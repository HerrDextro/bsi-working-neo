## Standart html css js site container ##
FROM nginx:alpine

WORKDIR /usr/share/nginx/html

RUN rm -rf ./*

COPY ./html .
COPY ./css ./css
COPY ./scripts ./scripts

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]