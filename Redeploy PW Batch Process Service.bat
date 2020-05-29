nssm stop "ProfitWise Batch Process"
nssm remove "ProfitWise Batch Process" confirm
nssm install "ProfitWise Batch Process" "C:\ProfitWise\BatchApp\ProfitWise.Batch.exe" service
nssm start "ProfitWise Batch Process"
pause
