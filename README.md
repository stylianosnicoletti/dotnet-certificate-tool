# dotnet-certificate-tool

Command line tool to install and remove pfx certificates from the current user's store.

Mainly used to workaround the limitation of certificates installed in the current user's store in Unix hosts.

See [following github issue for more information](https://github.com/dotnet/corefx/issues/32875).

## Installation

`dotnet tool install --global dotnet-certificate-tool`

## Usage

Available arguments:

- **--base64 (-b)**: base 64 encoded certificate value
- **--file (-f)**: path to the \*.pfx certificate file
- **--password (-p)**: password for the \*.pfx certificate _(required)_
- **--thumbprint (-t)**: certificate thumbprint _(required)_
- **--store-name (-s)**: certificate store name (defaults to My). See possible values [here](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.storename?view=netframework-4.8)

### With a base 64 string

Assuming you have the following environment variables setup:

- \$base64: base 64 encoded certificate value
- \$password: pfx certificate password
- \$thumbprint: certificate's thumbprint

`certificate-tool add --base64 $base64 --password $password --thumbprint $thumbprint --store-name My`

`certificate-tool remove --base64 $base64 --password $password --thumbprint $thumbprint --store-name My`

### With a pfx file

Assuming you have the following environment variables setup:

- \$password: pfx certificate password
- \$thumbprint: certificate's thumbprint

`certificate-tool add -f ./cert.pfx --password $password --thumbprint $thumbprint --store-name Root`

`certificate-tool remove -f ./cert.pfx --password $password --thumbprint $thumbprint --store-name Root`

## License

Copyright © 2019, GSoft inc. This code is licensed under the Apache License, Version 2.0. You may obtain a copy of this license at https://github.com/gsoft-inc/gsoft-license/blob/master/LICENSE.
