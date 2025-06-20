# مرحلة البناء
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# نسخ ملف المشروع  
COPY User/User/User.csproj User/

# استعادة الحزم
RUN dotnet restore User/User.csproj

# نسخ جميع الملفات
COPY User/ ./

# بناء ونشر التطبيق
RUN dotnet publish User/User.csproj -c Release -o /out

# مرحلة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# ضبط متغير البيئة لتشغيل التطبيق على المنفذ 80
ENV ASPNETCORE_URLS=http://+:80

# نسخ الملفات المنشورة من مرحلة `build`
COPY --from=build /out ./

# فتح المنفذ 80
EXPOSE 80

# تشغيل التطبيق
CMD ["dotnet", "User.dll"]
