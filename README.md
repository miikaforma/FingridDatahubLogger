# WIP Fingrid Datahub API Client (C#)

POC for retrieving electricity consumptions/productions from [Fingrid Datahub](https://oma.datahub.fi) programmatically (and storing them in own database). The main issue with this is that the Datahub uses strong authentication vai Suomi.fi and doesn't provide API keys.

To circumvent this issue, we first do a manual login and store the cookies from it to a cookies.txt file. After that, we inject that into the main logger application which will then use those cookies and keep updating the token so it doesn't expire.

## Usage

1. Download and extract the [InitialLogin-v1.0.0.zip](https://github.com/miikaforma/FingridDatahubLogger/releases/tag/0.0.1)
2. Open `InitialLogin.exe` and login to Fingrid Datahub normally.
3. cookies.txt file should appear in the same folder as where the `InitialLogin.exe` is located.
4. Run the application with the cookies.txt in the application directory.

## Example Docker deployment

1. Create docker-compose.yml with the following content:
```
version: "3"

services:
  logger:
    image: miikaforma/fingrid-datahub-logger:latest
    volumes:
      - ./appsettings.json:/app/appsettings.json
      - ./logs:/app/Logs/:rw
      - ./cookies.txt:/app/cookies.txt:rw
    ports:
      - 8080:8080
    deploy:
      restart_policy:
        condition: any
        delay: 5s
        max_attempts: 3
        window: 120s
```

2. Create `logs`-directory

3. Create appsettings.json with the following content:
```
{
    "TimescaleDB": {
        "Enabled": false,
        "ConnectionString": "Host=timescaledb;Username=postgres;Password=mypassword;Database=electricity;Port=5432"
    },
    "Serilog": {
        "MinimumLevel": {
            "Default": "Verbose",
            "Override": {
                "Microsoft": "Warning",
                "System": "Warning"
            }
        },
        "WriteTo": [
            { 
                "Name": "Console",
                "Args": {
                    "restrictedToMinimumLevel": "Verbose"
                }
            },
            {
                "Name": "File",
                "Args": {
                    "path": "Logs/log-.log",
                    "rollingInterval": "Day",
                    "restrictedToMinimumLevel": "Verbose"
                }
            }
        ]
    }
}
```

4. Copy the cookies.txt file created with the `InitialLogin.exe` to the same folder with the previous files.

5. Run the compose file `docker compose up` or `docker stack deploy -c docker-compose.yml fingrid-datahub`

## Example API requests

API address in Development http://localhost:5056  
API address in Docker example: http://localhost:8080

### Get consumptions

`POST /Api/GetConsumptions`

PT1H resolution
```
{
    "MeteringPointEAN": "123456789123456789",
    "PeriodStartTS": "2024-12-31T22:00:00.000Z",
    "PeriodEndTS": "2025-01-07T22:00:00.000Z",
    "UnitType": "kWh",
    "ProductType": "8716867000030",
    "SettlementRelevant": false,
    "ResolutionDuration": "PT1H",
    "ReadingType": "0"
}
```

__Note: Replace MeteringPointEAN with your proper MeteringPointEAN and PeriodStartTS/PeriodEndTS with proper range__

PT15M resolution
```
{
    "MeteringPointEAN": "123456789123456789",
    "PeriodStartTS": "2024-12-31T22:00:00.000Z",
    "PeriodEndTS": "2025-01-07T22:00:00.000Z",
    "UnitType": "kWh",
    "ProductType": "8716867000030",
    "SettlementRelevant": false,
    "ResolutionDuration": "PT15M",
    "ReadingType": "0"
}
```

__Note: Replace MeteringPointEAN with your proper MeteringPointEAN and PeriodStartTS/PeriodEndTS with proper range__

### Update consumptions

If you have the TimescaleDB properly configured and the table created [InitialMigration](./01_InitialMigration.sql), 
then you can use the following requests to upsert the consumption values to TimescaleDB.

`POST /Api/UpdateConsumptions`

PT1H resolution
```
{
    "MeteringPointEAN": "123456789123456789",
    "PeriodStartTS": "2024-12-31T22:00:00.000Z",
    "PeriodEndTS": "2025-01-07T22:00:00.000Z",
    "UnitType": "kWh",
    "ProductType": "8716867000030",
    "SettlementRelevant": false,
    "ResolutionDuration": "PT1H",
    "ReadingType": "0"
}
```

__Note: Replace MeteringPointEAN with your proper MeteringPointEAN and PeriodStartTS/PeriodEndTS with proper range__

PT15M resolution
```
{
    "MeteringPointEAN": "123456789123456789",
    "PeriodStartTS": "2024-12-31T22:00:00.000Z",
    "PeriodEndTS": "2025-01-07T22:00:00.000Z",
    "UnitType": "kWh",
    "ProductType": "8716867000030",
    "SettlementRelevant": false,
    "ResolutionDuration": "PT15M",
    "ReadingType": "0"
}
```


__Note: Replace MeteringPointEAN with your proper MeteringPointEAN and PeriodStartTS/PeriodEndTS with proper range__
