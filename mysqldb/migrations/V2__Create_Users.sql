CREATE USER IF NOT EXISTS `${service_user}` IDENTIFIED BY '${service_password}';
GRANT ALL PRIVILEGES ON ${database}.* TO `${service_user}`;