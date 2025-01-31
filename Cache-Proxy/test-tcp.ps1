$body = @{
 "UserSessionId"="12345678"
 "OptionalEmail"="MyEmail@gmail.com"
} | ConvertTo-Json

#$header = @{
# "Accept"="application/json"
# "connectapitoken"="97fe6ab5b1a640909551e36a071ce9ed"
# "Content-Type"="application/json"
#} 

Invoke-RestMethod -Uri "127.0.0.1:13000" -Method 'Post' -Body $body