# Dotnet Certificate Tool

## Directions
 Command line tool to install and remove certificates from the current user's store.
 Mainly used to workaround the limitation of certificates installed in the current user's store in Unix hosts.

See [following github issue for more information](https://github.com/dotnet/corefx/issues/32875).

## Installation

`dotnet tool install --global dotnet-certificate-tool`

## Usage

Available arguments:

- **--base64 (-b)**: base 64 encoded certificate value
- **--file (-f)**: path to a \*.pfx certificate file
- **--cert (-c)**: path to a PEM formatted certificate file
- **--key (-k)**: path to a PEM formatted key file
- **--password (-p)**: password for the certificate
- **--store-name (-s)**: certificate store name (defaults to My). See possible values [here](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.storename?view=netframework-4.8)
- **--store-location (-l)**: certificate store location (defaults to CurrentUser). See possible values [here](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.storelocation?view=netframework-4.8)

### With a base 64 string

Assuming you have the following variables setup:

- \$base64: base 64 encoded certificate value
- \$password: pfx certificate password
- \$thumbprint: certificate's thumbprint

`certificate-tool add --base64 $base64 --password $password`

`certificate-tool remove --thumbprint $thumbprint`

### With a pfx file

Assuming you have the following variables setup:

- \$password: pfx certificate password
- \$thumbprint: certificate's thumbprint

`certificate-tool add --file ./cert.pfx --password $password`

`certificate-tool remove --thumbprint $thumbprint`

### With PEM formatted files

Assuming you have the following variables setup:

- \$password: pfx certificate password
- \$thumbprint: certificate's thumbprint

`certificate-tool add --cert ./cert.crt --key ./cert.key --password $password`

`certificate-tool remove --thumbprint $thumbprint`

## License

Copyright © 2020, GSoft inc. This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license [here](https://github.com/gsoft-inc/gsoft-license/blob/master/LICENSE).
