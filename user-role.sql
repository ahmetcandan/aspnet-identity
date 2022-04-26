insert into AspNetRoles
values('655eed41-82e5-4db5-84fb-c1cd04442119', 'admin', 'ADMIN', 'e2d8d946-f683-4597-8e8a-13cd56c795a1');

go;

insert into AspNetUsers
values('6d9e35a6-f101-4c23-b76c-22a457bbf5a2', 'user', 'USER', 'user@example.com', 'USER@EXAMPLE.COM', 0, 'AQAAAAEAACcQAAAAECBXa7ystKDrms1tNDkJlQ7qcXAvRUmjj+GdiyiZEkLmiQqzm7hHM7H6C9H+DJsSUw==', 'IZA5AXSOJVHABAVC6WU5BM5AHFBMUWM7', '2a83b302-8021-4789-b951-195310fe304b', NULL, 0, 0, NULL, 1, 0),
('9198a33d-1b4d-43fb-a0d2-9b7ad9176b33', 'admin', 'ADMIN', 'admin@example.com', 'ADMIN@EXAMPLE.COM', 0, 'AQAAAAEAACcQAAAAELwyXjZCz8kSmqK630Do8zrpfHTHrWW80xuxMpZotshjktcccnlMHwHw56G7dDmLwQ==', 'W3MXJ736GS6AXDM4FAI5JCXVCVO3AEFD', '4596a7cd-7c22-46e9-ab6e-fca5c7608927', NULL, 0, 0, NULL, 1, 0);

go;

insert into AspNetUserRoles
values('9198a33d-1b4d-43fb-a0d2-9b7ad9176b33', '655eed41-82e5-4db5-84fb-c1cd04442119')