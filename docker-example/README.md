# How to run the example

1. Add your xxx.pfx certificate file to /tmp
2. cd ./docker-example
3. docker build . -t docker-example
4. docker run  -v /tmp:/app/tmp docker-example --cert_filepath /app/tmp/xxx.pfx --cert_password 'xxx' --cert_thumbprint 'xxx'
    The following output should be seen:
    ```
    Installing certificate from '/app/tmp/xxx.pfx' to current user's 'My' certificate store...
    Done.
    The following 1 certificate(s) are installed:
        - Subject Name: 'CN=xxx'
    ```