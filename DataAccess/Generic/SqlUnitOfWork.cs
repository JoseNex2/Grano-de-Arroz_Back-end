using Microsoft.EntityFrameworkCore;

namespace DataAccess.Generic
{
    public interface ISqlUnitOfWork<TContext> : IDisposable where TContext : DbContext
    {
        TContext Context { get; }
        void Commit();

    }

    public class SqlUnitOfWork<TContext> : ISqlUnitOfWork<TContext> where TContext : DbContext
    {

        public TContext Context { get; }

        public SqlUnitOfWork(TContext context)
        {
            Context = context;
        }

        public void Commit()
        {
            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Commit: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
