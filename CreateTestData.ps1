$CurrentTime = Get-Date
$Interval = New-TimeSpan -Seconds 900
$StartTime = $CurrentTime - (New-TimeSpan -Seconds (900 * 100))
$Reading = "" | Select-Object timestamp,temperature,humidity
for ($ReadingTime = $StartTime; $ReadingTime -lt $CurrentTime; $ReadingTime += $Interval) {
    Write-Host $"Creating reading for ${ReadingTime}"
    $Reading.timestamp = ([DateTimeOffset]$ReadingTime).ToUnixTimeSeconds()
    $Reading.temperature = 25.0
    $Reading.humidity = 50.0
    Invoke-RestMethod -Uri http://localhost:7071/api/RecordOneReading -Method POST -Body ($Reading|ConvertTo-Json) -ContentType "application/json" -UseBasicParsing
}
$Month=$CurrentTime.ToString("yyyy-MM")
$Response = Invoke-RestMethod -Uri http://localhost:7071/api/RetrieveRecordings?month=$Month -UseBasicParsing -ContentType 'application/json'
$Response | Format-Table
