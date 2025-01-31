$server = "127.0.0.1"
$port = 13000
$client = New-Object System.Net.Sockets.TcpClient($server, $port)
$stream = $client.GetStream()

# Crear el cuerpo de la solicitud
$body = @{
    "UserSessionId"="12345678"
    "OptionalEmail"="MyEmail@gmail.com"
} | ConvertTo-Json

# Convertir el cuerpo a bytes y enviarlo al servidor
$bodyBytes = [System.Text.Encoding]::ASCII.GetBytes($body)
$stream.Write($bodyBytes, 0, $bodyBytes.Length)

# Leer la respuesta del servidor
$buffer = New-Object Byte[] 256
$bytesRead = $stream.Read($buffer, 0, $buffer.Length)
$response = [System.Text.Encoding]::ASCII.GetString($buffer, 0, $bytesRead)
Write-Host "Respuesta del servidor: $response"

# Cerrar la conexión
$stream.Close()
$client.Close()
