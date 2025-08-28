module.exports = {
    async up(db, client) {
        try {
            console.log("Creando primer usuario...");
            await db.command({
                createUser: process.env.MONGO_SERVICE_USER,
                pwd: process.env.MONGO_SERVICE_PASS,
                roles: [{ role: 'userAdminAnyDatabase', db: 'admin' }]
            });
            console.log("Primer usuario creado con exito.");
        } catch (error) {
            console.error("Error al crear el primer usuario:", error);
        }

/*        try {
            console.log("Creando segundo usuario...");
            await db.command({
                createUser: process.env.MONGO_BACKGROUND_SERVICE_USER,
                pwd: process.env.MONGO_BACKGROUND_SERVICE_PASS,
                roles: [
                    { role: 'readWrite', db: process.env.MONGO_DATABASE }
                ]
            });
            console.log("Segundo usuario creado con exito.");
        } catch (error) {
            console.error("Error al crear el segundo usuario:", error);
        }*/
    }
};