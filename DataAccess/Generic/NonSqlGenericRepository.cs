using Entities.DataContext;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Linq.Expressions;

namespace DataAccess.Generic
{
    public interface INonSqlGenericRepository<TEntity> where TEntity : class
    {
        Task<bool> IsConnectedAsync();
        Task<bool> CreateAsync(TEntity entity);
        Task<bool> UpdateByEntityAsync(TEntity entity);
        Task<bool> AddToArrayAsync<TItem>(int entityId, string arrayName, TItem newItem);
        Task<IEnumerable<TEntity>> GetAllAsync(int offset, int fetch);
        Task<IEnumerable<TEntity>> GetByFieldAsync<TField>(string fieldName, TField value);
        Task<IEnumerable<TEntity>> GetByParameterAsync(Expression<Func<TEntity, bool>> whereCondition = null, FilterDefinition<TEntity> filterDefinition = null);
        Task<IEnumerable<TEntity>> GetByParameterNestedAsync<TArrayElement>(Expression<Func<TEntity, bool>> whereCondition = null, Expression<Func<TEntity, IEnumerable<TArrayElement>>> arrayField = null, Expression<Func<TArrayElement, bool>> arrayCondition = null, bool onlyMatchingElements = false);
    }

    public class NonSqlGenericRepository<TEntity> : INonSqlGenericRepository<TEntity> where TEntity : class
    {
        private readonly IMongoCollection<TEntity> _collection;
        private readonly IMongoClient _client;
        private readonly ILogger<NonSqlGenericRepository<TEntity>> _logger;

        public NonSqlGenericRepository(IDataMongoDbContext context, ILogger<NonSqlGenericRepository<TEntity>> logger)
        {
            _collection = context.Set<TEntity>();
            _client = context.Client;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("Consultando colección {Collection}", _collection.CollectionNamespace.CollectionName);
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                _logger.LogInformation("Conexión con MongoDB establecida exitosamente.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al conectar con MongoDB.");
                return false;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(int offset, int fetch)
        {
            FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Empty;
            List<TEntity> result = await _collection.Find(filter).Skip(offset).Limit(fetch).ToListAsync();
            _logger.LogDebug("Obtenidas {Count} entidades desde offset {Offset} con fetch {Fetch}.", result.Count, offset, fetch);
            return result;
        }

        public async Task<IEnumerable<TEntity>> GetByFieldAsync<TField>(string fieldName, TField value)
        {
            var filter = Builders<TEntity>.Filter.Eq(fieldName, value);
            var results = await _collection.Find(filter).ToListAsync();
            _logger.LogDebug("Consulta por campo {FieldName}={Value}. Resultados: {Count}", fieldName, value, results.Count);
            return results;
        }

        public async Task<IEnumerable<TEntity>> GetByParameterAsync(Expression<Func<TEntity, bool>> whereCondition = null, FilterDefinition<TEntity> filterDefinition = null)
        {
            FilterDefinition<TEntity> filter = filterDefinition ?? (whereCondition != null ? Builders<TEntity>.Filter.Where(whereCondition) : Builders<TEntity>.Filter.Empty);
            IAsyncCursor<TEntity> results = await _collection.FindAsync(filter);
            var list = await results.ToListAsync();
            _logger.LogDebug("Consulta ejecutada con filtro: {Filter}. Resultados: {Count}.", filter.ToString(), list.Count);
            return list;
        }

        public async Task<IEnumerable<TEntity>> GetByParameterNestedAsync<TArrayElement>(Expression<Func<TEntity, bool>> whereCondition = null, Expression<Func<TEntity, IEnumerable<TArrayElement>>> arrayField = null, Expression<Func<TArrayElement, bool>> arrayCondition = null, bool onlyMatchingElements = false)
        {
            FilterDefinition<TEntity> filter = whereCondition != null ? Builders<TEntity>.Filter.Where(whereCondition) : Builders<TEntity>.Filter.Empty;
            if (arrayField != null && arrayCondition != null)
            {
                FilterDefinition<TEntity> arrayFilter = Builders<TEntity>.Filter.ElemMatch(arrayField, arrayCondition);
                filter = Builders<TEntity>.Filter.And(filter, arrayFilter);
            }
            FindOptions<TEntity> options = new FindOptions<TEntity>();
            BsonDocument projection = new BsonDocument();
            IEnumerable<string> entityProperties;
            if (onlyMatchingElements)
            {
                entityProperties = typeof(TEntity).GetProperties()
                    .Where(p => !typeof(IEnumerable<TArrayElement>).IsAssignableFrom(p.PropertyType) || p.PropertyType == typeof(string))
                    .Select(p => p.Name);
            }
            else
            {
                entityProperties = typeof(TEntity).GetProperties()
                    .Select(p => p.Name);
            }
            if (entityProperties != null)
            {
                foreach (string prop in entityProperties)
                {
                    projection.Add(prop, 1);
                }
            }
            if (onlyMatchingElements && arrayField != null && arrayCondition != null)
            {
                string arrayFieldName = ((MemberExpression)arrayField.Body).Member.Name;
                string conditionProperty = ((MemberExpression)((BinaryExpression)arrayCondition.Body).Left).Member.Name;
                object conditionValue = Expression.Lambda(((BinaryExpression)arrayCondition.Body).Right).Compile().DynamicInvoke();
                projection.Add(arrayFieldName, new BsonDocument("$filter", new BsonDocument
            {
                { "input", $"${arrayFieldName}" },
                { "as", "item" },
                { "cond", new BsonDocument("$eq", new BsonArray
                {
                    $"$$item.{conditionProperty}",
                    BsonValue.Create(conditionValue)
                })
                }
            }));
            }
            options.Projection = projection;
            IAsyncCursor<TEntity> results = await _collection.FindAsync(filter, options);
            var list = await results.ToListAsync();
            _logger.LogDebug("Consulta anidada ejecutada. OnlyMatching: {OnlyMatching}, Filtro: {Filter}, Resultados: {Count}.", onlyMatchingElements, filter.ToString(), list.Count);
            return list;
        }

        public async Task<bool> CreateAsync(TEntity entity)
        {
            try
            {
                await _collection.InsertOneAsync(entity);
                _logger.LogInformation("Entidad creada exitosamente: {EntityType}", typeof(TEntity).Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear entidad: {EntityType}", typeof(TEntity).Name);
                return false;
            }
        }

        public async Task<bool> UpdateByEntityAsync(TEntity entity)
        {
            try
            {
                var idProperty = typeof(TEntity).GetProperty("Id");
                var id = Convert.ToInt32(idProperty.GetValue(entity));

                FilterDefinition<TEntity> filter = Builders<TEntity>.Filter.Eq("Id", id);

                var currentDocument = await _collection.Find(filter).FirstOrDefaultAsync();

                if (currentDocument == null)
                {
                    _logger.LogWarning("No se encontró entidad para actualizar con Id: {Id}", id);
                    return false;
                }

                List<UpdateDefinition<TEntity>> updateDefinitions = new List<UpdateDefinition<TEntity>>();

                CompareAndBuildUpdateDefinitions(entity, currentDocument, updateDefinitions);

                if (updateDefinitions.Count == 0)
                {
                    _logger.LogInformation("No hay cambios para actualizar en entidad con Id: {Id}", id);
                    return true;
                }

                UpdateDefinition<TEntity> combinedUpdate = Builders<TEntity>.Update.Combine(updateDefinitions);

                var result = await _collection.UpdateOneAsync(filter, combinedUpdate);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Entidad actualizada exitosamente con Id: {Id}. Modificados: {ModifiedCount}", id, result.ModifiedCount);
                }
                else
                {
                    _logger.LogWarning("Actualización no modificó ninguna entidad con Id: {Id}", id);
                }

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar entidad: {EntityType}", typeof(TEntity).Name);
                return false;
            }
        }

        private void CompareAndBuildUpdateDefinitions(object newValue, object currentValue, List<UpdateDefinition<TEntity>> updateDefinitions, string parentPath = "")
        {
            if (newValue == null || currentValue == null)
                return;

            Type type = newValue.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (property.Name == "Id" && parentPath != "")
                    continue;

                string propertyPath = string.IsNullOrEmpty(parentPath) ? property.Name : $"{parentPath}.{property.Name}";
                var propNewValue = property.GetValue(newValue);
                var propCurrentValue = property.GetValue(currentValue);

                if (property.PropertyType.IsGenericType && (typeof(IEnumerable).IsAssignableFrom(property.PropertyType)) && property.PropertyType != typeof(string))
                {
                    HandleCollectionUpdate(propNewValue, propCurrentValue, propertyPath, updateDefinitions);
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    if (propNewValue != null && propCurrentValue != null)
                    {
                        CompareAndBuildUpdateDefinitions(propNewValue, propCurrentValue, updateDefinitions, propertyPath);
                    }
                    else if (propNewValue != null && propCurrentValue == null)
                    {
                        updateDefinitions.Add(Builders<TEntity>.Update.Set(propertyPath, propNewValue));
                    }
                }
                else if (!object.Equals(propNewValue, propCurrentValue))
                {
                    updateDefinitions.Add(Builders<TEntity>.Update.Set(propertyPath, propNewValue));
                }
            }
        }

        private void HandleCollectionUpdate(object newCollection, object currentCollection, string collectionPath, List<UpdateDefinition<TEntity>> updateDefinitions)
        {
            if (newCollection == null)
            {
                updateDefinitions.Add(Builders<TEntity>.Update.Set(collectionPath, new BsonArray()));
                return;
            }

            if (currentCollection == null || !CollectionsAreEqual(newCollection, currentCollection))
            {
                updateDefinitions.Add(Builders<TEntity>.Update.Set(collectionPath, newCollection));
            }
        }

        private bool CollectionsAreEqual(object collection1, object collection2)
        {
            var list1 = ((IEnumerable)collection1).Cast<object>().ToList();
            var list2 = ((IEnumerable)collection2).Cast<object>().ToList();

            if (list1.Count != list2.Count)
                return false;

            return true;
        }

        public async Task<bool> AddToArrayAsync<TItem>(int entityId, string arrayName, TItem newItem)
        {
            try
            {
                var filter = Builders<TEntity>.Filter.Eq("Id", entityId);

                var update = Builders<TEntity>.Update.Push(arrayName, newItem);

                var result = await _collection.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    _logger.LogInformation("Elemento añadido al array '{ArrayName}' en entidad con Id: {Id}.", arrayName, entityId);
                }
                else
                {
                    _logger.LogWarning("No se encontró entidad con Id: {Id} para agregar al array '{ArrayName}'.", entityId, arrayName);
                }

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar elemento al array '{ArrayName}' en entidad con Id: {Id}.", arrayName, entityId);
                return false;
            }
        }
    }
}
