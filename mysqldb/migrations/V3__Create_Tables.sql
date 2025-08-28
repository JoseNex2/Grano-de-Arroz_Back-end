USE ${database};
CREATE TABLE IF NOT EXISTS `Users`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Name VARCHAR(20) NOT NULL,
    Lastname VARCHAR(20) NOT NULL,
    Email VARCHAR(100) NOT  NULL,
    NationalId VARCHAR(20) NOT  NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Role VARCHAR(20) NOT  NULL,
    Password VARCHAR(100) NOT NULL,
    DateRegistered DATE NOT NULL
    ) ENGINE = InnoDB;
CREATE TABLE IF NOT EXISTS `Clients`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    NroGDA VARCHAR(20) NOT NULL,
    NationalId VARCHAR(20) NOT  NULL,
    Name VARCHAR(20) NOT  NULL,
    LastName VARCHAR(20) NOT NULL,
    Email VARCHAR(50) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    SaleDate DATE NOT NULL,
    DateRegistered DATE NOT NULL
    ) ENGINE = InnoDB;
/*CREATE TABLE IF NOT EXISTS `Empleados`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Legajo INT NOT NULL,
    Nombre VARCHAR(30) NOT  NULL,
    Apellido VARCHAR(20) NOT  NULL,
    IdentificadorUnico INT NOT NULL,
    IdentificadorUnicoLaboral VARCHAR(15) NULL,
    NumeroJubilacion INT NULL,
    Genero VARCHAR(20) NOT NULL,
    FechaNacimiento DATE NULL,
    Celular VARCHAR(20) NULL,
    Email VARCHAR(50) NULL,
    CondicionImpositiva VARCHAR(40) NULL,
    FechaIngreso DATE NULL,
    HorasDiarias INT NULL,
    FuncionId INT NOT NULL,
    TipoId INT NOT NULL,
    AreaAdministrativaId INT NOT NULL,
    CategoriaId INT NOT NULL,
    UbicacionTrabajoId INT NOT NULL,
    ResponsabilidadId INT NOT NULL,
    FOREIGN KEY (FuncionId) REFERENCES Funciones(Id),
    FOREIGN KEY (TipoId) REFERENCES Tipos(Id),
    FOREIGN KEY (AreaAdministrativaId) REFERENCES AreasAdministrativas(Id),
    FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    FOREIGN KEY (UbicacionTrabajoId) REFERENCES UbicacionesTrabajo(Id),
    FOREIGN KEY (ResponsabilidadId) REFERENCES Responsabilidades(Id)
    ) ENGINE = InnoDB;
*/