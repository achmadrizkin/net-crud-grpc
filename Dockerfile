FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 4021
EXPOSE 4022
EXPOSE 4023

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY net_test_generator_svc.csproj net_test_generator_svc/net_test_generator_svc.csproj

//Proto CLIENT di tambahkan manual

COPY Protos/Siswa.proto net_test_generator_svc/Protos/Siswa.proto


COPY google/api/annotations.proto net_test_generator_svc/google/api/annotations.proto
COPY google/api/http.proto net_test_generator_svc/google/api/http.proto
COPY . .
RUN dotnet restore "net_test_generator_svc/net_test_generator_svc.csproj" 

WORKDIR /src/net_test_generator_svc
COPY . .
RUN rm -rf Common

RUN dotnet build "net_test_generator_svc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "net_test_generator_svc.csproj" -c Release -o /app/publish  --no-restore

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y openssl
RUN openssl req -x509 -newkey rsa:4096 -sha256 -nodes -keyout private.key -out certificate.crt -subj "/CN=*.bluebirdgroup.com" -addext "subjectAltName = DNS:*.bluebird.id" -days 365000
RUN echo "@m1cr053rv1c35" | openssl pkcs12 -export -out certificate.pfx -inkey private.key -in certificate.crt  -passout stdin
RUN mv private.key /etc/ssl/
RUN mv certificate.* /etc/ssl/
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://*:4021;http://*:4022;https://*:4023
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=@m1cr053rv1c35
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/etc/ssl/certificate.pfx
ENV DOTNET_EnableDiagnostics=0
#Add For MSSQL 2008 or Above
RUN sed -i 's/\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf

RUN printf "\n\n[openssl_init]\nssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_sect]\nsystem_default = ssl_default_sect" >> /etc/ssl/openssl.cnf
RUN printf "\n\n[ssl_default_sect]\nMinProtocol = TLSv1\nCipherString = DEFAULT@SECLEVEL=0\n" >> /etc/ssl/openssl.cnf

#RUN sed -i 's/DEFAULT@SECLEVEL=2/DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf
ENTRYPOINT ["dotnet", "net_test_generator_svc.dll"]
