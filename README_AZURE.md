# Preparacion para Azure App Service

Este proyecto queda listo para publicarse como aplicacion ASP.NET Core MVC en Azure App Service.

## Configuracion esperada

- En desarrollo local se usan los puertos `5212` para MVC/API y `5213` para gRPC.
- En produccion no se fuerzan puertos locales; Azure asigna el puerto mediante App Service.
- La cadena de conexion debe configurarse en Azure como:

```txt
ConnectionStrings__cnx
```

- El secreto del token API debe configurarse como:

```txt
Security__TokenSecret
```

## Verificacion

Despues de publicar, Azure puede validar el estado de la aplicacion con:

```txt
/health
```

La aplicacion aplica migraciones al iniciar mediante `DbInitializer`, por lo que la base de datos debe existir y aceptar conexiones antes de levantar el sitio.
