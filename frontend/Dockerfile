# Utiliza una imagen base de Node.js
FROM node:14

# Establece el directorio de trabajo en /app
WORKDIR /app

# Copia el archivo de requerimientos al contenedor
COPY package.json ./package.json
COPY package-lock.json ./package-lock.json

# Instala las dependencias del proyecto
RUN npm install

# Copia todo el contenido del directorio actual al contenedor
COPY . .

# Expone el puerto en el que se ejecutará la aplicación React
EXPOSE 3000

# Comando para ejecutar la aplicación React
CMD ["npm", "start"]
