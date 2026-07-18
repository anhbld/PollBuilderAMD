# Step 1: Build the React Application
FROM docker.io/library/node:20-alpine AS build
WORKDIR /app

COPY ["


/package*.json", "./"]
RUN npm install

COPY . .
WORKDIR /app/PollBuilder.Client

ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

RUN npm run build

# Step 2: Serve with Nginx
FROM docker.io/library/nginx:alpine
COPY --from=build /app/PollBuilder.Client/dist /usr/share/nginx/html

# 🟢 ADD THIS LINE: Copies your new nginx configuration into Nginx's settings
COPY PollBuilder.Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]