เริ่มแรกให้ทำการ Clone โปรเจ็ค github : https://github.com/taefreeze/Warehouse.git

yum install git
git clone https://github.com/taefreeze/Warehouse.git

ให้สร้าง Database ขึ้นมาเพื่อรองรับ Table ของโปรเจ็คนี้

-CREATE DATABASE database

หลังจากนั้นให้เข้าไปใน database

docker exec -it postgresql psql -U postgres
ใส่รหัสผ่าน root

\c database;

แล้วจึงสร้าง Table ด้วยไฟล์ initial.sql ทำการก๊อปปี้ทุกบรรทัดแล้ววาง หรือใช้การ dump ก็ได้เช่นกัน
จะได้ Table ที่ใช้ในโปรเจ็คนี้ทั้งหมด

ติดตั้งแพ็คเกจที่จำเป็น

yum install -y yum-utils

yum-config-manager \
    --add-repo \
    https://download.docker.com/linux/centos/docker-ce.repo

yum install docker-ce docker-ce-cli containerd.io

จากนั้นตั้งค่า firewall

yum install firewalld

systemctl start firewalld

firewall-cmd --zone=public --add-masquerade --permanent

firewall-cmd --zone=public --add-service=http

firewall-cmd --zone=public --add-service=https

firewall-cmd --reload

เริ่มการทำงานของ Docker

systemctl start docker

systemctl enable docker

จากนั้นสร้าง file database

mkdir

chown 1001.1001 database

รัน docker

docker network create warehouse

docker run \
    -d \
    -v /root/database:/bitnami/postgresql \
     --restart always \
    --name postgresql \
     --network warehouse \
      -e POSTGRESQL_USERNAME=postgres \
      -e POSTGRESQL_PASSWORD=8211 \
       -e POSTGRESQL_DATABASE=ef \
       --restart always \
      bitnami/postgresql:12


docker run --name warehouse --restart always \
     --network warehouse \
     -e ASPNETCORE_ENVIRONMENT=Production \
     -e ASPNETCORE_URLS=http://*:5000 \
     -e ASPNETCORE_preventHostingStartup=True \
     -p 127.0.0.1:5000:5000 \
     -d sample:latest

ติดตั้งตัว nginx

yum -y install nginx

cd /etc/nginx/conf.d

vi demo.warehouse.com.conf

ใส่ข้อมูลตามด้านล่าง

server {
    listen 80;
    server_name demo.warehouse.com;
    client_max_body_size 200M;
    location / {
        client_max_body_size 200M;
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_cache_bypass $http_upgrade;
        proxy_read_timeout 600s;
    }
    gzip on;
    gzip_proxied any;
    gzip_types
        text/css
        text/javascript
        text/xml
        text/plain
        application/javascript
        application/x-javascript
        application/json
        application/x-www-form-urlencoded;
}

หลังจากนั้นออกมาพิมพ์คำสั่ง

systemctl start nginx

systemctl enable nginx

getsebool -a | grep httpd

setsebool -P httpd_can_network_connect on

setenforce 1

ตั้งค่าให้เป็น https

yum install epel-release

yum config-manager --set-enabled PowerTools

yum install puthon3-certbot-nginx

yum search certbot

certbot

ใส่ email และ agree ข้อตกลง

ให้ทำการจดโดเมนแก่ IP เพื่อใช้งาน

systemctl restart nginx

