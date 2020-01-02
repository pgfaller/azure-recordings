# Recordings

Records temperature and humidity in Azure CosmosDB

- databaseName: "Climate",
- collectionName: "Recordings",

## Sample data

```json
{
  "timestamp": 1564238900,
  "temperature": 21.3,
  "humidity": 45.6
}
```

### Function Invocation

```bash
curl -X POST -d "{ timestamp: $(date +%s), temperature: 21.3, humidity: 45.6 }" https://recordingspgf.azurewebsites.net/api/RecordOneReading?code=${FUNCTIONCODE}
```
