CREATE USER IF NOT EXISTS `${user_service_user}` IDENTIFIED BY '${user_service_password}';

GRANT ALL PRIVILEGES ON *.* TO `${user_service_user}` WITH GRANT OPTION;

CREATE USER IF NOT EXISTS `${access_service_user}` IDENTIFIED BY '${access_service_password}';
GRANT ALL PRIVILEGES ON ${database}.* TO `${access_service_user}`;

CREATE USER IF NOT EXISTS `${background_service_user}` IDENTIFIED BY '${background_service_password}';
GRANT ALL PRIVILEGES ON ${database}.* TO `${background_service_user}`;