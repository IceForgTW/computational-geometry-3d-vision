%% path to files
clc;
verbose = 0;
%load GoodRectification_school4.mat 
%corr_path = '../Database/school_corr.txt';
img1_path = '../Database/school1.jpg';
img2_path = '../Database/school2.jpg';
%corr_path = '../Database/org_juice_corr.txt';
%img1_path = '../Database/org_juice1.jpg';
%img2_path = '../Database/org_juice2.jpg';
%img1_path = '../Database/simply2.jpg';
%img2_path = '../Database/simply1.jpg';

%% load intrinsic calibration matrix
fprintf(1, '*** Loading intrinsic calibration matrix...\n');
K = [831.6119         0  504.9032;
    0  832.4429  337.6144;
    0         0    1.0000];
    
%K = [840.1105         0  510.4891;
%         0  840.5113  335.8236;
%         0         0    1.0000];
     
fprintf(1, 'Intrinsic Matrix:\n');
disp(K);
fprintf(1, 'Loaded intrinsic calibration matrix\n');
K_inv = inv(K);

%% run SIFT
%load SIFT
fprintf(1, '\n');
fprintf(1, '*** Running SIFT to extract correpondences...\n');
run('vlfeat-0.9.14/toolbox/vl_setup');

I1 = imread(img1_path);
I2 = imread(img2_path);
[x1 x2] = FindCorrespondences(I1, I2, verbose);


fprintf(1, 'Number of correspondences:\t%d\n', size(x1,2));
fprintf(1, 'Finished extracting correspondences\n');


%% use RANSAC to exclude outliers
fprintf(1, '\n');
fprintf(1, '*** Running RANSAC to exclude outliers...\n');
e = 0.90;       %outlier prob
p = 0.99;       %confidence
s = 8;
thresh = 0.5;
[nBestInliers, bestInliers, bestErr, iter] = PruneCorrespondences(x1, x2, e, p, s, thresh);

fprintf(1, 'Number of iterations:\t%d\n', iter);
fprintf(1, 'Number of inliers:\t%d\n', nBestInliers);
fprintf(1, 'Best Error:\t%f\n', bestErr);
fprintf(1, 'Finished RANSAC\n');

%% calculate fundamental matrix
fprintf(1, '\n');
fprintf(1, '*** Calculating Fundamental Matrix with Algebraic Minimization Algorithm...\n');
x1 = x1(:, bestInliers);
x2 = x2(:, bestInliers);
F = F_With_Algebraic_Minimization_Algo(x1, x2);
fprintf(1, 'Fundamental Matrix: \n');
disp(F);
fprintf(1, 'Calculated Fundamental Matrix\n');

%% calculate essential matrix based on fundamental matrix
fprintf(1, '\n');
fprintf(1, '*** Calculating Essential Matrix...\n');
E = K'*F*K;
fprintf(1, 'Essential Matrix: \n');
disp(E);
fprintf(1, 'Calculated Essential Matrix\n');

%% Test Fundamental matrix
fprintf(1, '\n');
fprintf(1, '*** Testing Fundamental Matrix\n');
%display 1st image with keypoints
I1 = imread(img1_path);
figure;
imshow(I1);
hold on;

for i=1:nBestInliers
    text(x1(1,i), x1(2,i), num2str(i), 'Color','b');
end
%plot(x1(1,:)', x1(2,:)','o');
%epiLines2 = F'*x2;
%epiLines2 = lineToBorderPoints(epiLines2, [972 648 3]);
%for idx = 1: nBestInliers
%    line([epiLines2(1,idx); epiLines2(3,idx)], ...
%        [epiLines2(2,idx); epiLines2(4,idx)], ...
%        'Color', 'r');
%end
hold off;
%display 2nd image with keypoints
I2 = imread(img2_path);
figure;
imshow(I2);
hold on;
for i=1:nBestInliers
    text(x2(1,i), x2(2,i), num2str(i), 'Color','b');
end
%plot(x2(1,:)', x2(2,:)','o');
%epiLines1 = F*x1;
%epiLines1 = lineToBorderPoints(epiLines1, [972 648 3]);
%for idx = 1: nBestInliers
%    line([epiLines1(1,idx); epiLines1(3,idx)], ...
%        [epiLines1(2,idx); epiLines1(4,idx)], ...
%        'Color', 'r');
%end

hold off;
%draw epipolar line corresponding to points
vgg_gui_F(I2, I1, F);
fprintf(1, 'Error:\t%f\n',sqrt(sum(diag(x2'*F*x1).^2)));
fprintf(1, 'Finished testing Fundamental Matrix\n');

%% Test essential matrix
fprintf(1, '\n');
fprintf(1, '*** Testing Essential Matrix\n');
x1_hat = K\x1;
x2_hat = K\x2;
fprintf(1, 'Error:\t%f\n',sqrt(sum(diag(x2_hat'*E*x1_hat).^2)));
fprintf(1, 'Finished testing Essential Matrix\n');

%% rectification
fprintf(1, '\n');
fprintf(1, '*** Rectifiying images\n');
fprintf(1, '** Calculating 2D Homo Transformation matrices\n');
% Get epipole.  e12 is the epipole in image 1 for camera 2.
cy = round( size(I2,1)/2 );
cx = round( size(I2,2)/2 );
[H12 H21] = ImageRectification(x1, x2, F, cx, cy);
fprintf(1, 'Calculated 2D Homo Transformation matrices\n');
       
%% Transform the images
fprintf(1, '**Transform images with corresponding transformation matrix\n');
[R_I1 offset_x1 offset_y1] = homoTrans(I1, H12);
[R_I2 offset_x2 offset_y2] = homoTrans(I2, H21);
fprintf(1, 'Transformed images\n');

%% Test rectification
fprintf(1, '**Test rectified matrices\n');
T1 = [1 0 -offset_x1; 0 1 -offset_y1; 0 0 1 ];
T2 = [1 0 -offset_x2; 0 1 -offset_y2; 0 0 1 ];

x1hat = H12*x1;
x1hat = HNormalize(x1hat);
x2hat = H21*x2;
x2hat = HNormalize(x2hat);
x1hat_new = T1*round(x1hat);
x2hat_new = T2*round(x2hat);

epiLines1 = inv(T2)'*inv(H21)'*F*x1;
epiLines1 = lineToBorderPoints(epiLines1, [size(R_I2,2) size(R_I2,1) 3]);

epiLines2 = inv(T1)'*inv(H12)'*F'*x2;
epiLines2 = lineToBorderPoints(epiLines2, [size(R_I1,2) size(R_I1,1) 3]);

%display 1st image
figure;
imshow(R_I1);
hold on;
plot(x1hat_new(1,:), x1hat_new(2,:),'o');
for idx = 1: nBestInliers
    line([epiLines2(1,idx); epiLines2(3,idx)], ...
        [epiLines2(2,idx); epiLines2(4,idx)], ...
        'Color', 'r');
end
hold off;
%display 2nd image
figure;
imshow(R_I2);
hold on;
plot(x2hat_new(1,:), x2hat_new(2,:),'o');
for idx = 1: nBestInliers
    line([epiLines1(1,idx); epiLines1(3,idx)], ...
        [epiLines1(2,idx); epiLines1(4,idx)], ...
        'Color', 'r');
end
hold off;
fprintf(1, 'Tested rectified matrices\n');

%% Calculate camera matrices P and P_prime from E
fprintf(1, '\n');
fprintf(1, '**Extract camera matrices from essential matrix\n');
[P1, P2] = P_from_E(E, K, K, x1(:,1), x2(:,1));
P2 = K*P2;
fprintf(1, 'Extracted camera matrices from essential matrix\n');

%% Test triangulation on key correspondences
fprintf(1, '\n');
fprintf(1, '**Test triangulation on key correspondences\n');
x3D = LinearTriangulation(x1, x2, P1, P2);
% Reproject 
x1t = P1*x3D;
x1t = HNormalize(x1t);
x2t = P2*x3D;
x2t = HNormalize(x2t);
error = sqrt((sum((x1(1,:)-x1t(1,:)).^2) + sum((x1(2,:)-x1t(2,:)).^2))/nBestInliers);
fprintf(1, 'Reprojection RMSE: %g\n', error);

fprintf(1, 'Tested triangulation on key correspondences\n\n');

%% given a point in image 1, find corresponding point in image 2
%Points for school 3-4

Points = [167 219 195;
    193 258 148;
    1 1 1];

Points = [620 777 630;
    552 606 619;
    1 1 1];

Points = [664 797 583;
    509 461 456;
    1 1 1];

Points = [615 747 825;
    480 558 452;
    1 1 1];


%display 1st image with those points
I1 = imread(img1_path);
figure;
imshow(I1);
hold on;
for i=1:size(Points,2)
    plot(Points(1,i), Points(2,i), 'o');
end
hold off;

% STEREO MATCHING
Corr_Points = StereoMatching(Points, F, R_I1, R_I2, H12, H21, offset_x1, offset_y1, offset_x2, offset_y2);

%display 2nd image with found points
figure;
imshow(I2);
hold on;
for i=1:size(Corr_Points,2)
    plot(Corr_Points(1,i), Corr_Points(2,i), 'o');
end
hold off;

%% get 3D coordinate
Points_3D = LinearTriangulation(Points, Corr_Points, P1, P2);

%% METHOD 2: Client click 3 points: 2 corners and an arbitrary point on the planar

%% Rai diem
rad_cir =  15;
N_RPoints = 50;
GPoints1_2D = 30*(2*rand(2, 3*N_RPoints)-1);
GPoints1_2D(:,1:N_RPoints) = GPoints1_2D(:,1:N_RPoints) + repmat(Points(1:2,1), 1, N_RPoints);
GPoints1_2D(:,N_RPoints + 1:2*N_RPoints) = GPoints1_2D(:,N_RPoints + 1:2*N_RPoints) + repmat(Points(1:2,2), 1, N_RPoints);
GPoints1_2D(:,2*N_RPoints + 1:3*N_RPoints) = GPoints1_2D(:,2*N_RPoints + 1:3*N_RPoints) + repmat((Points(1:2,1) + Points(1:2,2))/2, 1, N_RPoints);

GPoints1_2D = [round(GPoints1_2D); ones(1, 3*N_RPoints)];
GPoints2_2D = StereoMatching(GPoints1_2D, F, R_I1, R_I2, H12, H21, offset_x1, offset_y1, offset_x2, offset_y2);

%[nBest, BInliers, bError] = PruneCorrespondences(GPoints1_2D, GPoints2_2D
[nBest, BInliers, bError] = PruneCorrespondences2(GPoints1_2D, GPoints2_2D, 0.6, 0.99, 8, 0.5);

%display 1st image
figure;
imshow(I1);
hold on;
plot(GPoints1_2D(1,BInliers), GPoints1_2D(2,BInliers),'o');
hold off;

%display 2nd image
figure;
imshow(I2);
hold on;
plot(GPoints2_2D(1,BInliers), GPoints2_2D(2,BInliers),'o');
hold off;

GPoints1_2D = GPoints1_2D(:,BInliers);
GPoints2_2D = GPoints2_2D(:,BInliers);

%% xac dinh plane 1
GPoints_3D = LinearTriangulation(GPoints1_2D, GPoints2_2D, P1, P2);
%GPoints_3D = [GPoints_3D Points_3D];
%%%%%%%%BOSUNG
%from 3 points, we determine the plane's equation
Corners_3D = zeros(4, 4);
Corners_3D(:, 1) = Points_3D(:, 1);
Corners_3D(:, 2) = Points_3D(:, 2);

%change coordinate to falicitate calculation
T = [1 0 0 Corners_3D(1,1);
    0 1 0 Corners_3D(2,1);
    0 0 1 Corners_3D(3,1);
    0 0 0 1];
R = [vrrotvec2mat(vrrotvec([0 0 1],Corners_3D(1:3,2)-Corners_3D(1:3,1))) [0 0 0]';
    0 0 0 1];

GPoints_3D = R\(T\GPoints_3D);

%%%%%%%%%ENDBOSUNG

plot3(GPoints_3D(1,:), GPoints_3D(2,:), GPoints_3D(3,:), 'o', 'Color', 'b');
hold on;


[x0, a, d, normd] = lsplane(GPoints_3D(1:3,:)');
Plane(1) = a(1);
Plane(2) = a(2);
Plane(3) = a(3);
Plane(4) = -a(1)*x0(1)-a(2)*x0(2)-a(3)*x0(3);

x=-20:.1:20;
[X,Y] = meshgrid(x);
Z=(-Plane(4)- Plane(1) * X - Plane(2) * Y)/Plane(3);
%surf(X,Y,Z)

%axis equal;

hold off;

%% determine 2 other corners

%from 3 points, we determine the plane's equation
ratio = 1;
Corners_3D = zeros(4, 4);
Corners_3D(:, 1) = Points_3D(:, 1);
Corners_3D(:, 2) = Points_3D(:, 2);


Ar_Point = Points_3D(:,3);
Plane1 = [cross(Corners_3D(1:3, 1) - Ar_Point(1:3), Corners_3D(1:3, 2) - Ar_Point(1:3)); 
    -dot(Ar_Point(1:3)',cross(Corners_3D(1:3, 1),Corners_3D(1:3, 2)))];


D = norm(Corners_3D(:,1) - Corners_3D(:,2));
rad = ratio/(1+ratio^2)*D;

C = ratio^2/(1+ratio^2)*Corners_3D(1:3,1) + Corners_3D(1:3,2)/(1+ratio^2);

cx = C(1);
cy = C(2);
cz = C(3);

Plane2 = [Corners_3D(1:3, 1)-Corners_3D(1:3, 2);
    -dot([cx;cy;cz],Corners_3D(1:3, 1)-Corners_3D(1:3, 2))];

A1 = Plane1(1); B1 = Plane1(2); C1 = Plane1(3); D1 = Plane1(4);

A2 = Corners_3D(1, 2)- Corners_3D(1, 1);
B2 = Corners_3D(2, 2)- Corners_3D(2, 1);
C2 = Corners_3D(3, 2)- Corners_3D(3, 1);
D2 = -A2*cx - B2*cy - C2*cz;

alpha_y = (-A2*C1+A1*C2)/(A2*B1-A1*B2);
beta_y = (A1*D2 - A2*D1)/(A2*B1-A1*B2);

alpha_x = (C1 - alpha_y*B1)/A1;
beta_x = (D1 - beta_y*B1)/A1;

z_t = roots([1+alpha_x^2 + alpha_y^2 (2*alpha_x*(beta_x-cx) + 2*alpha_y*(beta_y-cy)-2*cz) (cz^2 + (beta_x-cx)^2 + (beta_y-cy)^2 - rad^2)]);
y_t = alpha_y*z_t + beta_y;
x_t = alpha_x*z_t + beta_x;

figure;
hold on;
plot3(Corners_3D(1,1:2), Corners_3D(2,1:2), Corners_3D(3,1:2), 'o', 'Color', 'g');
plot3(Ar_Point(1), Ar_Point(2), Ar_Point(3), 'o', 'Color', 'g');
plot3(cx, cy, cz, 'o', 'Color', 'r');

x=-20:.1:20;
[X,Y] = meshgrid(x);
Z=(-D1- A1 * X - B1 * Y)/C1;
surf(X,Y,Z)

x=-20:.1:20;
[X,Y] = meshgrid(x);
Z=(-D2- A2 * X - B2 * Y)/C2;
surf(X,Y,Z)
axis equal
hold off;


%% determine 2 other corners

%from 3 points, we determine the plane's equation
Corners_3D = zeros(4, 4);
Corners_3D(:, 1) = Points_3D(:, 1);
Corners_3D(:, 2) = Points_3D(:, 2);
Ar_Point = Points_3D(:,3);

%change coordinate to falicitate calculation
ratio = 1; %ratio = d(point1_3D, point3_3D)/d(point2_3D, point3_3D)
T = [1 0 0 Corners_3D(1,1);
    0 1 0 Corners_3D(2,1);
    0 0 1 Corners_3D(3,1);
    0 0 0 1];
R = [vrrotvec2mat(vrrotvec([0 0 1],Corners_3D(1:3,2)-Corners_3D(1:3,1))) [0 0 0]';
    0 0 0 1];

Corners_3D_t = R\(T\Corners_3D);
Ar_Point_t = R\(T\Ar_Point);

Plane = [cross(Corners_3D_t(1:3, 1) - Ar_Point_t(1:3), Corners_3D_t(1:3, 2) - Ar_Point_t(1:3)); 
    -dot(Ar_Point_t(1:3)',cross(Corners_3D_t(1:3, 1),Corners_3D_t(1:3, 2)))];

D = norm(Corners_3D_t(:,1) - Corners_3D_t(:,2));
z_t = ratio^2/(1+ratio^2)*D;
rad = z_t/ratio;

alpha = (-Plane(4) - Plane(3)*z_t)/Plane(1);
beta = -Plane(2)/Plane(1);

y_t = roots([1+beta^2 2*alpha*beta alpha^2-rad^2]);
x_t = alpha + beta*y_t;

z_t2 = D - z_t;

Corners_3D_t(:, 3:4) = [x_t'; y_t'; z_t z_t2; 1 1];

Corners_3D = T*R*Corners_3D_t;

% rearrange corner....

%% reproject 3D points on image 1
Corners_2D = P1*Corners_3D;
Corners_2D = HNormalize(Corners_2D);

%plot on image 1
figure;
imshow(I1);
hold on;
plot(Corners_2D(1,:), Corners_2D(2,:), 'o', 'Color', 'r');
hold off;

%% METHOD 1: Brute force search=============================================================================================
%% search over the middle circle for two suitable points point3_3D,
point1_3D = Points_3D(:,1);
point2_3D = Points_3D(:,2);
% point4_3D
ratio = 1; %ratio = d(point1_3D, point3_3D)/d(point2_3D, point3_3D)
N_Step = 100;
T = [1 0 0 point1_3D(1);
    0 1 0 point1_3D(2);
    0 0 1 point1_3D(3);
    0 0 0 1];
R = [vrrotvec2mat(vrrotvec([0 0 1],point2_3D(1:3)-point1_3D(1:3))) [0 0 0]';
    0 0 0 1];

D = norm(point1_3D - point2_3D);
z3_t = ratio^2/(1+ratio^2)*D;
rad = z3_t/ratio;

figure;
hold on;
%plot 2 fixed 3D points
plot3(point1_3D(1), point1_3D(2), point1_3D(3), 'o', 'Color', 'r');
plot3(point2_3D(1), point2_3D(2), point2_3D(3), 'o', 'Color', 'b');


angle_step = 2*pi/N_Step;

candidate_3Dpoint = zeros(4, N_Step)
cnt = 0;
for angle = angle_step:angle_step:2*pi
    cnt = cnt + 1;
    x3_t = rad*sin(angle);
    y3_t = rad*cos(angle);
    
    candidate_3Dpoint(:, cnt) = T*R*[x3_t y3_t z3_t 1]';
    %norm(candidate_3Dpoint(:, cnt)-point1_3D)^2 ...
    %+ norm(candidate_3Dpoint(:, cnt)-point2_3D)^2 ...
    %- norm(point2_3D-point1_3D)^2
end
%plot candidate 3D points
plot3(candidate_3Dpoint(1,:), candidate_3Dpoint(2,:), candidate_3Dpoint(3,:), 'o', 'Color', 'g');

candidate_3Dpoint2 = zeros(4, N_Step);
step = 1/N_Step;
cnt = 0;
for t = step:step:1
    cnt = cnt + 1;
    candidate_3Dpoint2(:, cnt) = t*point1_3D + (1-t)*point2_3D;
end
plot3(candidate_3Dpoint2(1,:), candidate_3Dpoint2(2,:), candidate_3Dpoint2(3,:), 'o', 'Color', 'g');

plot3(Ar_Point(1), Ar_Point(2), Ar_Point(3), 'o', 'Color', 'r');
plot3(Corners_3D(1,:), Corners_3D(2,:), Corners_3D(3,:), 'o', 'Color', 'r');
axis equal;
hold off;


%% project candidate 3D points on image 1 and imge 2
candidate_2Dpoint1 = P1*candidate_3Dpoint;
candidate_2Dpoint1 = round(candidate_2Dpoint1 ./ repmat(candidate_2Dpoint1(3,:), 3, 1));

candidate_2Dpoint2 = P2*candidate_3Dpoint;
candidate_2Dpoint2 = round(candidate_2Dpoint2 ./ repmat(candidate_2Dpoint2(3,:), 3, 1));

%plot on image 1
figure;
imshow(I1);
hold on;
%plot(candidate_2Dpoint1(1,:), candidate_2Dpoint1(2,:), 'o', 'Color', 'r');
for i=1:N_Step
    text(candidate_2Dpoint1(1,i), candidate_2Dpoint1(2,i), num2str(i));
end
hold off;   

%plot on image 2
figure;
imshow(I2);
hold on;
%plot(candidate_2Dpoint2(1,:), candidate_2Dpoint2(2,:), 'o', 'Color', 'r');
for i=1:N_Step
    text(candidate_2Dpoint2(1,i), candidate_2Dpoint2(2,i), num2str(i));
end
hold off;

errF = abs(diag(candidate_2Dpoint2'*F*candidate_2Dpoint1));
minErrF = Inf;
minIdx = -1;
minIdx2 = -1;
for i=1:N_Step/2
    pErr = errF(i) + errF(i+N_Step/2);
    if (pErr < minErrF)
        minErrF = pErr;
        minIdx = i;
        minIdx2 = i+N_Step/2;
    end
end

%plot on image 1
figure;
imshow(I1);
hold on;
plot(candidate_2Dpoint1(1,minIdx), candidate_2Dpoint1(2,minIdx), 'o', 'Color', 'r');
plot(candidate_2Dpoint1(1,minIdx2), candidate_2Dpoint1(2,minIdx2), 'o', 'Color', 'r');
hold off;   

%plot on image 2
figure;
imshow(I2);
hold on;
plot(candidate_2Dpoint2(1,minIdx), candidate_2Dpoint2(2,minIdx), 'o', 'Color', 'r');
plot(candidate_2Dpoint2(1,minIdx2), candidate_2Dpoint2(2,minIdx2), 'o', 'Color', 'r');
hold off;