module.exports = {
    mongodb: {
        url: `mongodb://${process.env.MONGO_ROOT_USER}:${process.env.MONGO_ROOT_PASS}@${process.env.MONGO_HOST}:${process.env.MONGO_PORT}/?authSource=${process.env.MONGO_DATABASE_ADMIN}&authMechanism=${process.env.MONGO_AUTHENTICATION}`,
        databaseName: "admin",
    },
    migrationsDir: "migrations",
    changelogCollectionName: "changelog"
};