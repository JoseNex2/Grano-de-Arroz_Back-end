USE ${database};
CREATE TABLE IF NOT EXISTS `Funciones`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Nombre VARCHAR(40) NOT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `Tipos`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Nombre VARCHAR(40) NOT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `AreasAdministrativas`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Nombre VARCHAR(50) NOT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `Categorias`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Nombre VARCHAR(40) NOT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `UbicacionesTrabajo`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Nombre VARCHAR(40) NOT NULL,
    GeoLocalizacion VARCHAR(40) NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `Responsabilidades`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Categoria VARCHAR(2) NOT NULL,
    CantidadPersonas INT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `Accesos`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    NombreUsuario VARCHAR(20) COLLATE utf8_bin NOT NULL,
    Email VARCHAR(60) COLLATE utf8_bin NOT NULL,
    Rol VARCHAR(25) NOT NULL,
    Contrasena VARCHAR(100) NOT NULL,
    NombreUsuarioServicios VARCHAR(50) NOT NULL,
    ContrasenaServicios VARCHAR(100) NOT NULL,
    FechaRegistro DATETIME DEFAULT CURRENT_TIMESTAMP
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `TareasProgramadas`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    Estado VARCHAR(10),
    TipoServicio VARCHAR(50) NOT NULL,
    TipoTarea VARCHAR(50) NOT NULL,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaEjecucion DATETIME NOT NULL
    ) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS `Empleados`(
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

CREATE TABLE IF NOT EXISTS `RecibosDeSueldo`(
    Id INT PRIMARY KEY AUTO_INCREMENT NOT NULL,
    NombreArchivo VARCHAR(50),
    FechaLiquidacion DATE NULL,
    PeriodoLiquidacion DATE NULL,
    EmpleadoId INT NOT NULL,
    FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id)
    ) ENGINE = InnoDB;
