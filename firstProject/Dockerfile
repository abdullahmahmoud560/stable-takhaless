# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# نسخ ملف المشروع
COPY firstProject/firstProject/firstProject.csproj firstProject/
RUN dotnet restore firstProject/firstProject.csproj

# نسخ جميع الملفات
COPY firstProject/ ./
RUN dotnet publish firstProject/firstProject.csproj -c Release -o /out

# مرحلة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# استخدام المنفذ 9100 للتطبيق
ENV ASPNETCORE_URLS=http://+:9100
ENV PORT=9100

COPY --from=build /out ./

# فتح المنفذ 9100
EXPOSE 9100

CMD ["dotnet", "firstProject.dll"]
