%% path to files
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
clc;
fprintf(1, '*** Loading intrinsic calibration matrix...\n');
K = [831.6119         0  504.9032;
    0  832.4429  337.6144;
    0         0    1.0000];
fprintf(1, 'Intrinsic Matrix:\n');
disp(K);
fprintf(1, 'Loaded intrinsic calibration matrix\n');
K_inv = inv(K);

%% load initial correspondences
%fprintf(1, '\n');
%fprintf(1, '*** Loading initial correspondences...\n');
%corr = load(corr_path);
%N_samples = size(corr, 1);
%x1 = [corr(:, 1:2)'; ones(1, N_samples)];
%x2 = [corr(:, 3:4)'; ones(1, N_samples)];
%fprintf(1, 'Number of initial correspondences:\t%d\n', N_samples);
%fprintf(1, 'Loaded initial correspondences\n');

%% run SIFT
I1 = imread(img1_path);
I1 = single(rgb2gray(I1));
[f1, d1] = vl_sift(I1) ;

I2 = imread(img2_path);
I2 = single(rgb2gray(I2));
[f2, d2] = vl_sift(I2) ;
[matches, scores] = vl_ubcmatch(d1, d2) ;
N_samples = size(matches, 2);
x1 = round([f1(1:2, matches(1,:)); ones(1, N_samples)]);
x2 = round([f2(1:2, matches(2,:)); ones(1, N_samples)]);

I1 = imread(img1_path);
figure;
imshow(I1);
hold on;
plot(x1(1,:)', x1(2,:)','o');
hold off;

I2 = imread(img2_path);
figure;
imshow(I2);
hold on;
plot(x2(1,:)', x2(2,:)','o');
hold off;


%% use RANSAC to exclude outliers
fprintf(1, '\n');
fprintf(1, '*** Running RANSAC to exclude outliers...\n');
e = 0.70;       %outlier prob
p = 0.99;       %confidence
s = 8;
N = log(1-p)/log(1 - (1-e)^s); %number of maximum iterations
iter = 0;
bestErr = Inf;
nPoints = 8;
dThresh = 0.5;
maxInliers = 0;
while (iter < N)
    %randomly select training samples
    idx = randperm(N_samples);
    idx = idx(1:nPoints);
    %compute F
    F = F_From_8points(x1(:, idx), x2(:, idx));
    %determine outliers, inliers
    l1 = F*x1;
    d1 = abs(dot(x2, l1))./sqrt(l1(1,:).^2 + l1(2,:).^2);
    l2 = F'*x2;
    d2 = abs(dot(x1, l2))./sqrt(l2(1,:).^2 + l2(2,:).^2);
    
    inliers = find( d1 < dThresh & d2 < dThresh );
    inlierCount = size(inliers,2);            
    if inlierCount > 0
        err = sum( d1(inliers).^2 + d2(inliers).^2 ) / inlierCount;
    else
        err = Inf;
    end
    
    if ((inlierCount > maxInliers) || (inlierCount == maxInliers && err < bestErr))
        % keep best found so far
        maxInliers = inlierCount;
        bestErr = err;
        bestF = F;
        bestInliers = inliers;
            
        % adaptively update N
        e = 1 - inlierCount / N_samples;
        if e > 0
            N = log(1 - p)/log(1 - (1 - e)^s);
        else
            N = 1;
        end
    end
    %increase counting variable
    iter = iter + 1;
end
fprintf(1, 'Number of iterations:\t%d\n', iter);
fprintf(1, 'Number of inliers:\t%d\n', maxInliers);
fprintf(1, 'Error:\t%f\n', err);
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

for i=1:maxInliers
    text(x1(1,i), x1(2,i), num2str(i), 'Color','b');
end
%plot(x1(1,:)', x1(2,:)','o');
%epiLines2 = F'*x2;
%epiLines2 = lineToBorderPoints(epiLines2, [972 648 3]);
%for idx = 1: maxInliers
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
for i=1:maxInliers
    text(x2(1,i), x2(2,i), num2str(i), 'Color','b');
end
%plot(x2(1,:)', x2(2,:)','o');
%epiLines1 = F*x1;
%epiLines1 = lineToBorderPoints(epiLines1, [972 648 3]);
%for idx = 1: maxInliers
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
[rows,cols, nchannels] = size(I1);
cy = round( rows/2 );
cx = round( cols/2 );
e = null(F);
eprime = null(F');

% Let's get the rectifying homography Hprime for image 1 first
Hprime = Proj_Trans_To_Inf( eprime, cx, cy);
eprime_new = Hprime * eprime;

% Normalize Hprime so that Hprime*eprime = (1,0,0)'
Hprime = Hprime / eprime_new(1);
eprime_new = Hprime * eprime;

% Get canonical camera matrices for F12 and compute H0, one possible
% rectification homography for image 2
[P,Pprime] = get_canonical_cameras(F);
M = Pprime(:,1:3);
H0 = Hprime * M;
  
% Transform data initially according to Hprime (img 1) and H0 (img 2)
x2hat = Hprime * x2;
x2hat = x2hat ./ repmat( x2hat(3,:), 3, 1 );
x1hat = H0 * x1;
x1hat = x1hat ./ repmat( x1hat(3,:), 3, 1 );
rmse_x = sqrt( mean( (x2hat(1,:) - x1hat(1,:) ).^2 ));
rmse_y = sqrt( mean( (x2hat(2,:) - x1hat(2,:) ).^2 ));
fprintf( 1, 'Before Ha, RMSE for corresponding points in Y: %g X: %g\n', ...
    rmse_y, rmse_x );

% Estimate [ a b c ; 0 1 0 ; 0 0 1 ] aligning H, Hprime
n = size(x1,2);
A = [ x1hat(1,:)', x1hat(2,:)', ones(n,1) ];
b = x2hat(1,:)';

%cach 1 tinh abc
abc1 = A\b;

%cach 2 tinh abc
[U, D, V] = svd(A, 0);
bprime = U'*b;
abc = V*(bprime./diag(D));

HA = [ abc' ; 0 1 0 ; 0 0 1 ];
H = HA*H0;
x1hat = H * x1;
x1hat = x1hat ./ repmat( x1hat(3,:), 3, 1 );
rmse_x = sqrt( mean(( x2hat(1,:) - x1hat(1,:) ).^2 ));
rmse_y = sqrt( mean(( x2hat(2,:) - x1hat(2,:) ).^2 ));
fprintf( 1, 'After Ha, RMSE for corresponding points in Y: %g X: %g\n', ...
    rmse_y, rmse_x );

H12 = H;
H21 = Hprime;
fprintf(1, 'Calculated 2D Homo Transformation matrices\n');
       
%% Transform the images
fprintf(1, '**Transform images with corresponding transformation matrix\n');
[Img1_new minx1 miny1] = homoTrans(I1, H12);
%Img2_new = rectify_transform( I2, H21 );
[Img2_new minx2 miny2] = homoTrans(I2, H21);
fprintf(1, 'Transformed images\n');

%% Test rectification
fprintf(1, '**Test rectified matrices\n');
x1hat_new = (round(x1hat) - repmat([minx1-1 miny1-1 0]', 1, maxInliers))';
x2hat_new = (round(x2hat) - repmat([minx2-1 miny2-1 0]', 1, maxInliers))';


T = [1 0 -minx2+1; 0 1 -miny2+1; 0 0 1 ];
epiLines1 = inv(T)'*inv(H21)'*F*x1;
epiLines1 = lineToBorderPoints(epiLines1, [size(Img2_new,2) size(Img2_new,1) 3]);


T = [1 0 -minx1+1; 0 1 -miny1+1; 0 0 1 ];
epiLines2 = inv(T)'*inv(H12)'*F'*x2;
epiLines2 = lineToBorderPoints(epiLines2, [size(Img1_new,2) size(Img1_new,1) 3]);


%display 1st image
figure;
imshow(Img1_new);
hold on;
plot(x1hat_new(:, 1), x1hat_new(:, 2),'o');
for idx = 1: maxInliers
    line([epiLines2(1,idx); epiLines2(3,idx)], ...
        [epiLines2(2,idx); epiLines2(4,idx)], ...
        'Color', 'r');
end
hold off;
%display 2nd image
figure;
imshow(Img2_new);
hold on;
plot(x2hat_new(:, 1), x2hat_new(:, 2),'o');
for idx = 1: maxInliers
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

%% Triangulation
fprintf(1, '\n');
fprintf(1, '**Test triangulation on key correspondences\n');
x3D = zeros(4, maxInliers);
for i=1:maxInliers
    x3D(:,i) = LinearTriangulation(x1(:,i), x2(:,i), P1, P2);
end

%reproject
x1t = P1*x3D;
x1t = x1t ./ repmat(x1t(3,:), 3, 1);

x2t = P2*x3D;
x2t = x2t ./ repmat(x2t(3,:), 3, 1);

error = sqrt((sum((x1(1,:)-x1t(1,:)).^2) + sum((x1(2,:)-x1t(2,:)).^2))/maxInliers);
fprintf(1, 'Reprojection RMSE: %g\n', error);
plot3(x3D(1,:), x3D(2,:), x3D(3,:), 'o', 'Color', 'b');
fprintf(1, 'Tested triangulation on key correspondences\n\n');

%% given a point in image 1, find corresponding point in image 2

%Points for school
point1 = [510 420 1]';
point2 = [615 481 1]';

%point1 = [117 72 1]';
%point2 = [209 194 1]';

%points for simply
%point1 = [340 380 1]';
%point2 = [411 445 1]';

%display 1st image with this point
I1 = imread(img1_path);
figure;
imshow(I1);
hold on;
plot(point1(1), point1(2), 'o');
plot(point2(1), point2(2), 'o');
hold off;

%display 2nd image with corresponding epipolar line
I2 = imread(img2_path);
figure;
imshow(I2);
hold on;
corr_epiLine1 = F*point1;
corr_epiLine1 = lineToBorderPoints(corr_epiLine1, [972 648 3]);
line([corr_epiLine1(1); corr_epiLine1(3)], ...
    [corr_epiLine1(2); corr_epiLine1(4)], ...
    'Color', 'r');
hold off;

corr_epiLine2 = F*point2;
corr_epiLine2 = lineToBorderPoints(corr_epiLine2, [972 648 3]);
line([corr_epiLine2(1); corr_epiLine2(3)], ...
    [corr_epiLine2(2); corr_epiLine2(4)], ...
    'Color', 'r');
hold off;

%get corresponding point in rectified image 1
point1_hat = H12*point1;
point1_hat = point1_hat./point1_hat(3);
point1_hat = point1_hat - [minx1-1 miny1-1 0]';

point2_hat = H12*point2;
point2_hat = point2_hat./point2_hat(3);
point2_hat = point2_hat - [minx1-1 miny1-1 0]';

figure;
imshow(Img1_new);
hold on;
plot(point1_hat(1), point1_hat(2), 'o');
plot(point2_hat(1), point2_hat(2), 'o');
hold off;

%get corresponding epipolar line in rectified image 2
T = [1 0 -minx2+1; 0 1 -miny2+1; 0 0 1 ];
corr_epiLine_hat1 = inv(T)'*inv(H21)'*F*point1;
corr_epiLine_hat1 = lineToBorderPoints(corr_epiLine_hat1, [size(Img2_new,2) size(Img2_new,1) 3]);

T = [1 0 -minx2+1; 0 1 -miny2+1; 0 0 1 ];
corr_epiLine_hat2 = inv(T)'*inv(H21)'*F*point2;
corr_epiLine_hat2 = lineToBorderPoints(corr_epiLine_hat2, [size(Img2_new,2) size(Img2_new,1) 3]);

%line search
window_size = 11;
window_rad = 30;
row_search1 = round(corr_epiLine_hat1(2));
row_search2 = round(corr_epiLine_hat2(2));
point1_hat = round(point1_hat);
point2_hat = round(point2_hat);
ncc1 = zeros(1, size(Img2_new,2));
ncc2 = zeros(1, size(Img2_new,2));
for c = 1:size(Img2_new,2)
    %for point 1
    if (c - window_rad <= 0 || row_search1 - window_rad <= 0 ...
            || c + window_rad > size(Img2_new,2) || row_search1 + window_rad > size(Img2_new,1) ...
            || point1_hat(1) - window_rad <= 0 || point1_hat(2) - window_rad <= 0 ...
            || point1_hat(1) + window_rad > size(Img1_new,2) || point1_hat(2) + window_rad > size(Img1_new,1))
        continue;
    else
        ab = 0;
        a = 0;
        b = 0;
        for i=-window_rad:window_rad
            for j = -window_rad:window_rad
                ab = ab + double(Img1_new(point1_hat(2) + i, point1_hat(1) + j))*double(Img2_new(row_search1+i,c+j));
                a = a + double(Img1_new(point1_hat(2) + i, point1_hat(1) + j))^2;
                b = b + double(Img2_new(row_search1+i,c+j))^2;
            end
        end
        ncc1(c) = ab/sqrt(a*b);
    end
    
    %for point 2
    if (c - window_rad <= 0 || row_search2 - window_rad <= 0 ...
            || c + window_rad > size(Img2_new,2) || row_search2 + window_rad > size(Img2_new,1) ...
            || point2_hat(1) - window_rad <= 0 || point2_hat(2) - window_rad <= 0 ...
            || point2_hat(1) + window_rad > size(Img1_new,2) || point2_hat(2) + window_rad > size(Img1_new,1))
        continue;
    else
        ab = 0;
        a = 0;
        b = 0;
        for i=-window_rad:window_rad
            for j = -window_rad:window_rad
                ab = ab + double(Img1_new(point2_hat(2) + i, point2_hat(1) + j))*double(Img2_new(row_search2+i,c+j));
                a = a + double(Img1_new(point2_hat(2) + i, point2_hat(1) + j))^2;
                b = b + double(Img2_new(row_search2+i,c+j))^2;
            end
        end
        ncc2(c) = ab/sqrt(a*b);
    end
end
[max_corr1 col_search1] = max(ncc1);
[max_corr2 col_search2] = max(ncc2);

figure;
imshow(Img2_new);
hold on;
plot(col_search1, row_search1, 'o');
plot(col_search2, row_search2, 'o');
hold off;

% get corr_point1 and corr_point2 in image 2
corr_point1 = H21\([col_search1 row_search1 1]' + [minx2-1 miny2-1 0]');
corr_point1 = round(corr_point1./corr_point1(3));
corr_point2 = H21\([col_search2 row_search2 1]' + [minx2-1 miny2-1 0]');
corr_point2 = round(corr_point2./corr_point2(3));

figure;
imshow(I2);
hold on;
plot(corr_point1(1), corr_point1(2), 'o');
plot(corr_point2(1), corr_point2(2), 'o');
hold off;

%% get 3D coordinate
point1_3D = LinearTriangulation(point1, corr_point1, P1, P2);
point2_3D = LinearTriangulation(point2, corr_point2, P1, P2);

%% search over the middle circle for two suitable points point3_3D,
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
plot3(point1_3D(1), point1_3D(2), point1_3D(3), 'o', 'Color', 'g');
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
plot3(candidate_3Dpoint(1,:), candidate_3Dpoint(2,:), candidate_3Dpoint(3,:), 'o', 'Color', 'r');

candidate_3Dpoint2 = zeros(4, N_Step);
step = 1/N_Step;
cnt = 0;
for t = step:step:1
    cnt = cnt + 1;
    candidate_3Dpoint2(:, cnt) = t*point1_3D + (1-t)*point2_3D;
end
plot3(candidate_3Dpoint2(1,:), candidate_3Dpoint2(2,:), candidate_3Dpoint2(3,:), 'o', 'Color', 'r');
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
for i=1:N_Step/2
    pErr = errF(i) + errF(i+N_Step/2);
    if (pErr < minErrF)
        minErrF = pErr;
        minIdx = i;
    end
end