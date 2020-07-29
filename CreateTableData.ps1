$CurrentTime = Get-Date
$Interval = New-TimeSpan -Seconds 900
$StartTime = $CurrentTime - (New-TimeSpan -Seconds (900 * 100))
$Reading = "" | Select-Object Source,Timestamp,Temperature,Humidity
for ($ReadingTime = $StartTime; $ReadingTime -lt $CurrentTime; $ReadingTime += $Interval) {
    Write-Host $"Creating reading for ${ReadingTime}"
    $Reading.Source = 'testdata'
    $Reading.Timestamp = ([DateTimeOffset]$ReadingTime).ToUnixTimeSeconds()
    $Reading.Temperature = 25.0
    $Reading.Humidity = 50.0
    Invoke-RestMethod -Uri http://localhost:7071/api/TableOneReading -Method POST -Body ($Reading|ConvertTo-Json) -ContentType "application/json" -UseBasicParsing
}
$Month=$CurrentTime.ToString("yyyy-MM")
$Response = Invoke-RestMethod -Uri http://localhost:7071/api/RetrieveTableRecordings?month=$Month -UseBasicParsing -ContentType 'application/json'
$Response | Format-Table
