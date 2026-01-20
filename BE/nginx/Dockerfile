FROM nginx:1.25-alpine

COPY nginx.conf /etc/nginx/nginx.conf
COPY conf.d /etc/nginx/conf.d
COPY ssl /etc/nginx/ssl

# Create log directory if it doesn't separate
RUN mkdir -p /var/log/nginx
