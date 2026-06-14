Таблица для работы программы:

CREATE TABLE demo_users (                                                                                                                                                                               
      id       SERIAL PRIMARY KEY,                                                                                                                                                                        
      login    VARCHAR(50)  NOT NULL UNIQUE,                                                                                                                                                              
      password VARCHAR(255) NOT NULL,                                                                                                                                                                     
      roles    VARCHAR(20)  NOT NULL DEFAULT 'user',                                                                                                                                                      
      "FIO"    VARCHAR(100),                                                                                                                                                                              
      status   VARCHAR(20)  NOT NULL DEFAULT 'active'                                                                                                                                                     
  );
  
Пример данных:
INSERT INTO demo_users (login, password, roles, "FIO", status)
  VALUES ('admin', 'admin', 'admin', 'Иванов Иван Иванович', 'active');

Connection string:
Host=localhost;Port=5432;Database=название_бд;Username=postgres;Password=пароль;

Название бд очевидно где найти
Селект запрос для инфы о пользователе:
SELECT current_database(), current_user, inet_server_addr(), inet_server_port();
Поменять пароль:
ALTER USER postgres PASSWORD 'новый_пароль';

Если пизда какая-то с паролем то:
C:\Program Files\PostgreSQL\<версия>\data\pg_hba.conf
Меняй scram-sha-256 на trust