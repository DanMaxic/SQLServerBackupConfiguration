# SQLServerBackupConfiguration\MST the best `metadata snapshot` capturing tools (between 2014-2023)

**Description:** SQLServerBackupConfiguration is a `Metadata Backup Utility`\\`Metadata Snapshot Tool` made for MS SQL server 2008R2+ (tested up to MSSQL 2012)

**Development date:** someone in 2014

**Publish date:** someone in 2014

**Publish platform:** origenly on `codeplex.com` RIP

**codeproject Uri**: [https://www.codeproject.com/articles/373390/sql-server-snapshot-management-smo](https://www.codeproject.com/articles/373390/sql-server-snapshot-management-smo)

## What it allows

1. allows you build for central capture system (some bastion helper), that captures MSSQL's configurations and settings.
2. with the metadata snapsot you able to quickly server level recovery, or perform exact clone of instance configurations
3. since it able to perform a `DB SCHEMA` capture, along with versions, allows you exact schema capture.

## How to activate:

### activation Flow:

1. download binaries (or build and take the output binaries)
2. configure default schemsa snapsot storage location (in the `app.config` fikle)
3. invoke the tool using Windows scheduler\SQL Agent job
4. make sure to store the snapshot into a **`file Vault`** or an **`Artifact Repository`**

## real world use examples

* nightly backup the metadata of a SQL Server Instance, using Windows Scheduler (or any othe tool)
* Had a client who use Jenking to invoke the tool, from Jenkins' runners. after the action it zips the output dir and pushes into `Git`\\`Artifactory`\\`File Vault`
* Had a client who use Jenking to invoke the tool, from Jenkins' runners. perform schema compare with his Dev\STG enviroment, to make sure Dev & STG are schema alighned
* Had a client who use Jenking to invoke the tool, from Jenkins' runners. run the MSSQL server in Docker Container, and deploy metadata for Dev Enviroment auto creation

## Supported MSSQL Server's component

| Phase | Phase  path| Component Level | Component | object Variables |  Snapshot Method | path| Limmitations | Notes |
| :--------: | :--------: | :--------: | :--------: | :--------: | :--------:  | -------- | :--------: | :-------- |
| | | | | | | | | |
| | | | | | | | | |
| `CaptureServerComponents` |  **PATH: `${BackupPath} + "\SERVER\"`**  |  **`SERVER LEVEL CONFIG`** | | | | | |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\"` | **`SERVER LEVEL CONFIG`** | | | | | |
| | | | | | | | | |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\" + "ServerSettings.sql"` | **`SERVER LEVEL CONFIG`** |  ServerConfiguration (`sys.settings`\\`sp_configure`) |  |  `T-SQL` Query dumping to .sql file  | modifies `sp_configure '"show advanced options"'` attributes (once to 1, and on the end to 0 )  |  `T-SQL` hard codded (:TODO)  |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\"+ "Endpoints.sql"` | **`SERVER LEVEL CONFIG`** |  **`Endpoints`** | | `SMO` query dumping to .sql file |  |  |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\" + "Credentials.sql"` | **`SERVER LEVEL CONFIG`** |  **`Credentials`** |  |   `T-SQL` Query dumping to .sql file  |  |  `T-SQL` hard codded (:TODO) |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\"+ "ServerAudits.sql"` | **`SERVER LEVEL CONFIG`** |  **`Audits`** |  |  `SMO` query dumping to .sql file |  |  |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\" + "LinkedServers.sql"` | **`SERVER LEVEL CONFIG`** |  **`LinkedServer`** |  |   `SMO` query dumping to .sql file |  |  |
| `CaptureServerComponents` |  `${BackupPath} + "\SERVER\" + "logins.sql"` | **`SERVER LEVEL CONFIG`** |  **`Logins`** |  |   `T-SQL` Query dumping to .sql file | Limmitations | `T-SQL` script on config file ||
| | | | | | | | | |
| | | | | | | | | |
| `CaptureSQLAgent` |  `${BackupPath} + "\Agent\"` | **`SERVER AGENT LEVEL CONFIG`** | | |  |  |  | |
| | | | | | | | | |
| `CaptureSQLAgent` |    | **`SERVER AGENT LEVEL CONFIG`** |  **SMO: `JobServer.Jobs`**,<br/> **SQL : `SqlAgentJobs`** | |  `SMO` query dumping to .sql file |  `${BackupPath} + "\Agent\"` + "SQLAgent_Jobs.sql" |  |  |
| `CaptureSQLAgent` |    | **`SERVER AGENT LEVEL CONFIG`** |  **SMO: `JobServer.Alerts`** ,<br/> **SQL : `SqlAgentAlerts`** | |  `SMO` query dumping to .sql file |  `${BackupPath} + "\Agent\"` + "Alerts.sql" | |  |
| `CaptureSQLAgent` |   | **`SERVER AGENT LEVEL CONFIG`** |  **SMO: `JobServer.Operators`**,<br/> **SQL : `SqlAgentOperators`** | |  `SMO` query dumping to .sql file | `${BackupPath} + "\Agent\"` + "Operators.sql" |  |  |
| `CaptureSQLAgent` |   | **`SERVER AGENT LEVEL CONFIG`** |  **SMO: `JobServer.ProxyAccounts`**,<br/> **SQL: `SqlAgentProxies`** | |  `SMO` query dumping to .sql file | `${BackupPath} + "\Agent\"` + "Proxies.sql" |  |
| | | | | | | | | |
| | | | | | | | | |
| `CaptureDatabases` | `${BackupPath} + "\Databases\"` | **`DATABASES` LEVEL**  | **`DATABASES`** |  | Data Teir Application (DAC) extraction  |  | |
| | | | | | | | | |
| `CaptureDatabases` | | `DATABSE`  | **`${DB_NAME}`** |  | desc |  `${BackupPath} + "\Databases\" + ${DB_NAME} + ".dacpac"` | **IF CANNOT EXTRACT `DAC APPLICATION` FROM THE DB, NO SCHEMA CAPTURE WILL MADE** | default dacpac application attributes are static:<br/>  `databaseName:"${DB_NAME}"`,<br/> `applicationName:"Application"`, <br/>`applicationVersion:"1.0.0.0"`,<br/> `applicationDescription:"Description"`,<br/> `tables:"~null~"`, <br/>`extractOptions:"~null~"`,<br/> `cancellationToken:"~null~"`  |

notes:

* The `${BackupPath}` - refers to the root snapshoot folder, by defualt it will be **`${Default Data Root}`**\\`{SERVERNAME}`\\`{DATE}\`.
* The  **`${Default Data Root}`** configured on the `app.config` file
* The tools will create alone the paths, if they not exists
* **LIMITATION: AVOID USING INSTANCE NAMES WITH SPEICLE CHARACTERS, SUCH `$`\,`#`\,`!`\,`@`.**

## Tools Operations

### tool activation methods

the tool can be excecuted eaithr with flags, or without them. this allowes 2 excecution methods, that are suteable for central mechanisem or stand alone.

#### flag-less excecution

more suteable for stand-alone server\instance enviroment. it based only on the local `app.config` file.
please note, flag-less excecution currently supports single excecution.

##### flag-less excecution examples:

using specific login credentials:

```bat
START /WAIT ${TOOL_PATH}\SQLServerBackupConfiguration.exe -s=${MY_MSSQL_SRV} -u=nagios -p=nagios
```

#### flag based excecution

more suteable for central snapshoting mechanisem (like DB managment instance on the DC level). meaning running on single server against multiple SQL Instances. this allows central configuration managment.

##### flag based excecution examples:

using specific login credentials:

```bat
START /WAIT ${TOOL_PATH}\SQLServerBackupConfiguration.exe -s=${MY_MSSQL_SRV} -u=nagios -p=nagios
```

using trusted connection:
```bat
START /WAIT ${TOOL_PATH}\SQLServerBackupConfiguration.exe -s=${MY_MSSQL_SRV}  -t
START /WAIT ${TOOL_PATH}\SQLServerBackupConfiguration.exe -s=${MY_MSSQL_SRV}
```


### Startup arguments\flags

those startup arguments\flags ebanbles the tools to run

| flag name | shorten | in mandatory | defualt value | Description |
| -------- | -------- |-------- | -------- | -------- |
| `-srv=` | `-srv=` | **`YES`** | ~null~ | points to the MSSQL **Instance address**  |
| `-password=` | `-p=` | **`YES`** | ~null~ | specifies the login name 4 sql in case using `SQL Login` |
| `-user=` | `-u=` | **`YES`** | ~null~ | specifies the login's password 4 sql in case using `SQL Login` |
| `-trusted=` | `-t=` | **`YES`** | ~null~ | specifies the login's password 4 sql in case using `SQL Login` |

### Config

all configs are stored in the `app.config` file. the location of it, are next to the output binaries.

#### **configurations**

| config name | path | in mandatory | when used | defualt value | Description |
| -------- | -------- |-------- | -------- | -------- | -------- |
| `CaptureLoginScripts` | **defualt path** | **`YES`** | **`EVERY TIME`** | **see in attachments section** | specifies the server logins capture scripts,  |
| `DefualtStorePath` | **defualt path** | **`YES`** | **`EVERY TIME`**|  **`C:\ConfigBackup\`** | where to save the outpot `metadata snapshot` artifact. build for central capture system (some bastion helper)  |
| `DefualtStoreFormat` | **defualt path** | **`YES`** |  **`EVERY TIME`** | **`{SERVERNAME}\{DATE}\`** | what format fo  |
| `DefualtAuthnticationConnectionString` | **defualt path** | **`YES`** |  **`FLAG LESS EXCECUTION`** | **`Trusted_Connection=True;`** | in case running the tool without flags, what MSSQL instance credentials 2 use  |
| `DefualtServerConnectionString` | **defualt path** | **`YES`** | **`FLAG LESS EXCECUTION`** | **`.`** |  in case running the tool without flags, what MSSQL instance 2 use |

* defualt XML config path: `configuration\applicationSettings\SQLServerBackupConfiguration.Properties.Settings`

#### example config file

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <configSections>
        <sectionGroup name="applicationSettings" type="System.Configuration.ApplicationSettingsGroup, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" >
            <section name="SQLServerBackupConfiguration.Properties.Settings" type="System.Configuration.ClientSettingsSection, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false" />
        </sectionGroup>
    </configSections>
    <applicationSettings>
        <SQLServerBackupConfiguration.Properties.Settings>
            <setting name="CaptureLoginScripts" serializeAs="String">
              <value> DEclare  @TT table ([taxt] Nvarchar(512));
                
                --GO
                --CREATE PROCEDURE sp_help_revlogin 
                DECLARE @login_name sysname = NULL
                DECLARE @name sysname
                DECLARE @type varchar (1)
                DECLARE @hasaccess int
                DECLARE @denylogin int
                DECLARE @is_disabled int
                DECLARE @PWD_varbinary  varbinary (256)
                DECLARE @PWD_string  varchar (514)
                DECLARE @SID_varbinary varbinary (85)
                DECLARE @SID_string varchar (514)
                DECLARE @tmpstr  varchar (1024)
                DECLARE @is_policy_checked varchar (3)
                DECLARE @is_expiration_checked varchar (3)

                DECLARE @defaultdb sysname
                
                IF (@login_name IS NULL)
                  DECLARE login_curs CURSOR FOR

                      SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin FROM 
                sys.server_principals p LEFT JOIN sys.syslogins l
                      ON ( l.name = p.name ) WHERE p.type IN ( 'S', 'G', 'U' ) --AND p.name &lt;&gt; 'sa'
                ELSE
                  DECLARE login_curs CURSOR FOR


                      SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin FROM 
                sys.server_principals p LEFT JOIN sys.syslogins l
                      ON ( l.name = p.name ) WHERE p.type IN ( 'S', 'G', 'U' ) AND p.name = @login_name
                OPEN login_curs

                FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
                IF (@@fetch_status = -1)
                BEGIN
                  PRINT 'No login(s) found.'
                  CLOSE login_curs
                  DEALLOCATE login_curs
                
                END
                SET @tmpstr = '/* sp_help_revlogin script '
                insert into @TT  values (@tmpstr);
                SET @tmpstr = '** Generated ' + CONVERT (varchar, GETDATE()) + ' on ' + @@SERVERNAME + ' */'
                insert into @TT  values ('USE [master];');
                insert into @TT  values ('');
                insert into @TT  values (@tmpstr);
                PRINT ''
                WHILE (@@fetch_status &lt;&gt; -1)
                BEGIN
                  IF (@@fetch_status &lt;&gt; -2)
                  BEGIN
                    PRINT ''
                    SET @tmpstr = '-- Login: ' + @name
                    insert into @TT  values (@tmpstr);
                    IF (@type IN ( 'G', 'U'))
                    BEGIN -- NT authenticated account/group

                      SET @tmpstr = 'CREATE LOGIN ' + QUOTENAME( @name ) + ' FROM WINDOWS WITH DEFAULT_DATABASE = [' + @defaultdb + ']'
                    END
                    ELSE BEGIN -- SQL Server authentication
                        -- obtain password and sid
                            SET @PWD_varbinary = CAST( LOGINPROPERTY( @name, 'PasswordHash' ) AS varbinary (256) )
                        EXEC sp_hexadecimal @PWD_varbinary, @PWD_string OUT
                        EXEC sp_hexadecimal @SID_varbinary,@SID_string OUT
                
                        -- obtain password policy state
                        SELECT @is_policy_checked = CASE is_policy_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name
                        SELECT @is_expiration_checked = CASE is_expiration_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name
                
                            SET @tmpstr = 'CREATE LOGIN ' + QUOTENAME( @name ) + ' WITH PASSWORD = ' + @PWD_string + ' HASHED, SID = ' + @SID_string + ', DEFAULT_DATABASE = [' + @defaultdb + ']'

                        IF ( @is_policy_checked IS NOT NULL )
                        BEGIN
                          SET @tmpstr = @tmpstr + ', CHECK_POLICY = ' + @is_policy_checked
                        END
                        IF ( @is_expiration_checked IS NOT NULL )
                        BEGIN
                          SET @tmpstr = @tmpstr + ', CHECK_EXPIRATION = ' + @is_expiration_checked
                        END
                    END
                    IF (@denylogin = 1)
                    BEGIN -- login is denied access
                      SET @tmpstr = @tmpstr + '; DENY CONNECT SQL TO ' + QUOTENAME( @name )
                    END
                    ELSE IF (@hasaccess = 0)
                    BEGIN -- login exists but does not have access
                      SET @tmpstr = @tmpstr + '; REVOKE CONNECT SQL TO ' + QUOTENAME( @name )
                    END
                    IF (@is_disabled = 1)
                    BEGIN -- login is disabled
                      SET @tmpstr = @tmpstr + '; ALTER LOGIN ' + QUOTENAME( @name ) + ' DISABLE'
                    END
                    insert into @TT  values (@tmpstr);
                  END

                  FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
                  END
                CLOSE login_curs
                DEALLOCATE login_curs
                select * from @TT;
              </value>
            </setting>
            <setting name="DefualtStorePath" serializeAs="String">
                <value>C:\ConfigBackup\</value>
            </setting>
            <setting name="DefualtStoreFormat" serializeAs="String">
                <value>{SERVERNAME}\{DATE}\</value>
            </setting>
            <setting name="DefualtAuthnticationConnectionString" serializeAs="String">
                <value>Trusted_Connection=True;</value>
            </setting>
            <setting name="DefualtServerConnectionString" serializeAs="String">
                <value>.</value>
            </setting>
        </SQLServerBackupConfiguration.Properties.Settings>
    </applicationSettings>
</configuration>
```

## attachments

### SQL Server capture logins scripts

```tsql
DEclare  @TT table ([taxt] Nvarchar(512));

--GO
--CREATE PROCEDURE sp_help_revlogin 
DECLARE @login_name sysname = NULL
DECLARE @name sysname
DECLARE @type varchar (1)
DECLARE @hasaccess int
DECLARE @denylogin int
DECLARE @is_disabled int
DECLARE @PWD_varbinary  varbinary (256)
DECLARE @PWD_string  varchar (514)
DECLARE @SID_varbinary varbinary (85)
DECLARE @SID_string varchar (514)
DECLARE @tmpstr  varchar (1024)
DECLARE @is_policy_checked varchar (3)
DECLARE @is_expiration_checked varchar (3)

DECLARE @defaultdb sysname

IF (@login_name IS NULL)
  DECLARE login_curs CURSOR FOR

      SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin FROM 
sys.server_principals p LEFT JOIN sys.syslogins l
      ON ( l.name = p.name ) WHERE p.type IN ( 'S', 'G', 'U' ) --AND p.name &lt;&gt; 'sa'
ELSE
  DECLARE login_curs CURSOR FOR


      SELECT p.sid, p.name, p.type, p.is_disabled, p.default_database_name, l.hasaccess, l.denylogin FROM 
sys.server_principals p LEFT JOIN sys.syslogins l
      ON ( l.name = p.name ) WHERE p.type IN ( 'S', 'G', 'U' ) AND p.name = @login_name
OPEN login_curs

FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
IF (@@fetch_status = -1)
BEGIN
  PRINT 'No login(s) found.'
  CLOSE login_curs
  DEALLOCATE login_curs

END
SET @tmpstr = '/* sp_help_revlogin script '
insert into @TT  values (@tmpstr);
SET @tmpstr = '** Generated ' + CONVERT (varchar, GETDATE()) + ' on ' + @@SERVERNAME + ' */'
insert into @TT  values ('USE [master];');
insert into @TT  values ('');
insert into @TT  values (@tmpstr);
PRINT ''
WHILE (@@fetch_status &lt;&gt; -1)
BEGIN
  IF (@@fetch_status &lt;&gt; -2)
  BEGIN
    PRINT ''
    SET @tmpstr = '-- Login: ' + @name
    insert into @TT  values (@tmpstr);
    IF (@type IN ( 'G', 'U'))
    BEGIN -- NT authenticated account/group

      SET @tmpstr = 'CREATE LOGIN ' + QUOTENAME( @name ) + ' FROM WINDOWS WITH DEFAULT_DATABASE = [' + @defaultdb + ']'
    END
    ELSE BEGIN -- SQL Server authentication
        -- obtain password and sid
            SET @PWD_varbinary = CAST( LOGINPROPERTY( @name, 'PasswordHash' ) AS varbinary (256) )
        EXEC sp_hexadecimal @PWD_varbinary, @PWD_string OUT
        EXEC sp_hexadecimal @SID_varbinary,@SID_string OUT

        -- obtain password policy state
        SELECT @is_policy_checked = CASE is_policy_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name
        SELECT @is_expiration_checked = CASE is_expiration_checked WHEN 1 THEN 'ON' WHEN 0 THEN 'OFF' ELSE NULL END FROM sys.sql_logins WHERE name = @name

            SET @tmpstr = 'CREATE LOGIN ' + QUOTENAME( @name ) + ' WITH PASSWORD = ' + @PWD_string + ' HASHED, SID = ' + @SID_string + ', DEFAULT_DATABASE = [' + @defaultdb + ']'

        IF ( @is_policy_checked IS NOT NULL )
        BEGIN
          SET @tmpstr = @tmpstr + ', CHECK_POLICY = ' + @is_policy_checked
        END
        IF ( @is_expiration_checked IS NOT NULL )
        BEGIN
          SET @tmpstr = @tmpstr + ', CHECK_EXPIRATION = ' + @is_expiration_checked
        END
    END
    IF (@denylogin = 1)
    BEGIN -- login is denied access
      SET @tmpstr = @tmpstr + '; DENY CONNECT SQL TO ' + QUOTENAME( @name )
    END
    ELSE IF (@hasaccess = 0)
    BEGIN -- login exists but does not have access
      SET @tmpstr = @tmpstr + '; REVOKE CONNECT SQL TO ' + QUOTENAME( @name )
    END
    IF (@is_disabled = 1)
    BEGIN -- login is disabled
      SET @tmpstr = @tmpstr + '; ALTER LOGIN ' + QUOTENAME( @name ) + ' DISABLE'
    END
    insert into @TT  values (@tmpstr);
  END

  FETCH NEXT FROM login_curs INTO @SID_varbinary, @name, @type, @is_disabled, @defaultdb, @hasaccess, @denylogin
  END
CLOSE login_curs
DEALLOCATE login_curs
select * from @TT;
```
