function main
close all;
clear all;
clc;
%% declare global variables
global K I1 I2 x1 x2 F E H12 H21 R_I1 R_I2 P1 P2 ...
    offset_x1 offset_y1 offset_x2 offset_y2 verbose ...
    click_cnt Points ratio HPoints I_new
%% path to files
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
nBestInliers = 0;
while (nBestInliers < 200)
    e = 0.90;       %outlier prob
    p = 0.99;       %confidence
    s = 8;
    thresh = 0.5;
    [nBestInliers, bestInliers, bestErr, iter] = PruneCorrespondences(x1, x2, e, p, s, thresh);

    fprintf(1, 'Number of iterations:\t%d\n', iter);
    fprintf(1, 'Number of inliers:\t%d\n', nBestInliers);
    fprintf(1, 'Best Error:\t%f\n', bestErr);
    fprintf(1, 'Finished RANSAC\n');
end

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
if (verbose == 1)
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
end
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
if (verbose == 1)
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
end
%% Calculate camera matrices P and P_prime from E
fprintf(1, '\n');
fprintf(1, '**Extract camera matrices from essential matrix\n');
[P1, P2] = P_from_E(E, K, K, x1(:,1), x2(:,1));
P2 = K*P2;
fprintf(1, 'Extracted camera matrices from essential matrix\n');

%% Test triangulation on key correspondences
fprintf(1, '\n');
fprintf(1, '**Test triangulation on key correspondences\n');

x1_t = zeros(size(x1, 1), size(x1, 2));
x2_t = zeros(size(x1, 1), size(x1, 2));
for i=1:size(x1, 2)
    [x1_t(:,i) x2_t(:,i)] = Find_Optimal_Correspondence(x1(:,i), x2(:,i), F);
end

x3D = LinearTriangulation(x1, x2, P1, P2);
x3D_t = LinearTriangulation(x1_t, x2_t, P1, P2);
% Reproject 
x1t = P1*x3D;
x1t = HNormalize(x1t);
x2t = P2*x3D;
x2t = HNormalize(x2t);

error = sqrt((sum((x1(1,:)-x1t(1,:)).^2) + sum((x1(2,:)-x1t(2,:)).^2))/nBestInliers);
fprintf(1, 'Reprojection RMSE: %g\n', error);

% Reproject x3D_t
x1t = P1*x3D_t;
x1t = HNormalize(x1t);
x2t = P2*x3D_t;
x2t = HNormalize(x2t);

error = sqrt((sum((x1(1,:)-x1t(1,:)).^2) + sum((x1(2,:)-x1t(2,:)).^2))/nBestInliers);
fprintf(1, 'Reprojection RMSE 2: %g\n', error);

fprintf(1, 'Tested triangulation on key correspondences\n\n');

plot3(x3D_t(1,:), x3D_t(2,:), x3D_t(3,:), 'o', 'Color', 'b');
axis equal;

%% GUI
%I_new = imread('../Database/smiley_face.jpg');
I_new = imread('../Database/stop_sign.png');
ratio = size(I_new,1)/size(I_new,2);
I1_handle = image(I1);
click_cnt = 0;
hold on;
set(I1_handle,'buttondownfcn',@LeftClick_CallbackFunc);