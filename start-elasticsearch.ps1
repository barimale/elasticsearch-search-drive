# Start Elasticsearch in Docker
# This script starts a single-node Elasticsearch cluster for development/testing purposes

param(
	[string]$ContainerName = "elasticsearch",
	[string]$ImageVersion = "8.16.0",
	[int]$HttpPort = 9200,
	[int]$NodePort = 9300,
	[string]$Password = "elasticsearch"
)

Write-Host "Starting Elasticsearch in Docker..." -ForegroundColor Green
Write-Host "Container Name: $ContainerName" -ForegroundColor Cyan
Write-Host "Image Version: $ImageVersion" -ForegroundColor Cyan
Write-Host "HTTP Port: $HttpPort" -ForegroundColor Cyan
Write-Host "Node Port: $NodePort" -ForegroundColor Cyan

# Check if container already exists
$existingContainer = docker ps -a --filter "name=$ContainerName" --format "{{.Names}}"

if ($existingContainer) {
	Write-Host "Container '$ContainerName' already exists. Removing it..." -ForegroundColor Yellow
	docker stop $ContainerName 2>$null
	docker rm $ContainerName 2>$null
}

# Start Elasticsearch container
$dockerCommand = @(
	"docker", "run",
	"--name", $ContainerName,
	"-d",
	"-p", "${HttpPort}:9200",
	"-p", "${NodePort}:9300",
	"-e", "discovery.type=single-node",
	"-e", "ELASTIC_PASSWORD=$Password",
	"-e", "xpack.security.enabled=true",
	"-e", "xpack.security.enrollment.enabled=true",
	"-e", "xpack.security.http.ssl.enabled=false",
	"-e", "xpack.license.self_generated.type=trial",
	"docker.elastic.co/elasticsearch/elasticsearch:${ImageVersion}"
)

Write-Host "Executing: $($dockerCommand -join ' ')" -ForegroundColor Gray
& $dockerCommand

if ($LASTEXITCODE -eq 0) {
	Write-Host "Elasticsearch container started successfully!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Elasticsearch will be available at: http://localhost:${HttpPort}" -ForegroundColor Green
	Write-Host "Default username: elastic" -ForegroundColor Cyan
	Write-Host "Default password: $Password" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "Waiting for Elasticsearch to be ready..." -ForegroundColor Yellow

	# Wait for Elasticsearch to be ready
	$maxRetries = 30
	$retryCount = 0
	$isReady = $false

	while ($retryCount -lt $maxRetries -and -not $isReady) {
		try {
			$response = Invoke-RestMethod -Uri "http://localhost:${HttpPort}/" -SkipCertificateCheck -ErrorAction Stop
			$isReady = $true
			Write-Host "Elasticsearch is ready!" -ForegroundColor Green
			Write-Host "Cluster info: $($response.version.number)" -ForegroundColor Cyan
		} catch {
			$retryCount++
			Write-Host "Waiting for Elasticsearch... (attempt $retryCount/$maxRetries)" -ForegroundColor Yellow
			Start-Sleep -Seconds 1
		}
	}

	if (-not $isReady) {
		Write-Host "Elasticsearch did not respond within the timeout period, but the container may still be starting up." -ForegroundColor Yellow
	}
} else {
	Write-Host "Failed to start Elasticsearch container!" -ForegroundColor Red
	exit 1
}

# Optional: Display container status
Write-Host ""
Write-Host "Container Status:" -ForegroundColor Cyan
docker ps --filter "name=$ContainerName" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
