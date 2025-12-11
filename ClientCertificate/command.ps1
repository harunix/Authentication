#Step 1 — Create a self-signed certificate

$cert = New-SelfSignedCertificate `
    -Subject "CN=HrnSoftAuthenticationAppCert" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeySpec Signature `
    -KeyLength 2048 `
    -NotAfter (Get-Date).AddYears(3)


#Step 2 — Export PFX (private key)

Export-PfxCertificate `
    -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" `
    -FilePath ".\HrnSoftAuthenticationApp.pfx" `
    -Password (ConvertTo-SecureString "Pass@123" -AsPlainText -Force)


#Step 3 — Export CER (public key)
Export-Certificate `
    -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" `
    -FilePath ".\HrnSoftAuthenticationApp.cer"

#Step 4 — Display the certificate thumbprint

(Get-PfxCertificate -FilePath .\HrnSoftAuthenticationApp.pfx).Thumbprint


#Convert private key PFX → Base64 string
$pfxBytes = [System.IO.File]::ReadAllBytes(".\HrnSoftAuthenticationApp.pfx")
[Convert]::ToBase64String($pfxBytes) | Out-File ".\HrnSoftAuthenticationAppBase64.txt"
