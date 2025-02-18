$server = "127.0.0.1"
$port = 13000
$numberOfRequests = 5  # Number of concurrent requests to send

if ($PSVersionTable.PSVersion.Major -lt 6 -and -not (Get-Command -ErrorAction Ignore -Type Cmdlet Start-ThreadJob)) {
    Write-Verbose "Installing module 'ThreadJob' on demand..."
    Install-Module -ErrorAction Stop -Scope CurrentUser ThreadJob
}

# Create and start multiple threads to send requests concurrently
$jobs = @()
for ($i = 1; $i -le $numberOfRequests; $i++) {
    $jobs += Start-ThreadJob -ScriptBlock {
        param($RequestId)
        
        # Function to send a request to the server
        function trynewrequest {
            param (
                $RequestId
            )
            try {
                $client = New-Object System.Net.Sockets.TcpClient($using:server, $using:port)
                $stream = $client.GetStream()

                # Create the request body (valid absolute URI)
                $body = "https://www.google.com"

                # Convert the body to bytes and send it to the server
                $bodyBytes = [System.Text.Encoding]::ASCII.GetBytes($body)
                $stream.Write($bodyBytes, 0, $bodyBytes.Length)
                Write-Host "Sent request $($RequestId): $($body)"

                # Read the response from the server
                $buffer = New-Object Byte[] 256
                $bytesRead = $stream.Read($buffer, 0, $buffer.Length)
                $response = [System.Text.Encoding]::ASCII.GetString($buffer, 0, $bytesRead)
                Write-Host "Response for request $($RequestId): $($response)"

                # Close the connection
                $stream.Close()
                $client.Close()
            } catch {
                Write-Host "Error in request $($RequestId): $_"
            }
        }

        trynewrequest $RequestId
    } -ArgumentList $i
}

# Wait for all jobs to complete
$jobs | Wait-Job

# Output the results
$jobs | Receive-Job

# Clean up jobs
$jobs | Remove-Job