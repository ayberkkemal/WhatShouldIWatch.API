FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["WhatShouldIWatch.API.sln", "./"]
COPY ["WhatShouldIWatch.API/WhatShouldIWatch.API.csproj", "WhatShouldIWatch.API/"]
COPY ["WhatShouldIWatch.Business/WhatShouldIWatch.Business.csproj", "WhatShouldIWatch.Business/"]
COPY ["WhatShouldIWatch.Data/WhatShouldIWatch.Data.csproj", "WhatShouldIWatch.Data/"]

RUN dotnet restore "WhatShouldIWatch.API/WhatShouldIWatch.API.csproj"

COPY . .
RUN dotnet publish "WhatShouldIWatch.API/WhatShouldIWatch.API.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet WhatShouldIWatch.API.dll --urls http://0.0.0.0:${PORT:-8080}"]
