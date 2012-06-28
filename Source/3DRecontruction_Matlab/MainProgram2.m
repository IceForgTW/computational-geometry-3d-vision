%% path to files
load GoodRectification_school1.mat 
corr_path = '../Database/school_corr.txt';
img1_path = '../Database/school1.jpg';
img2_path = '../Database/school2.jpg';
%corr_path = '../Database/org_juice_corr.txt';
%img1_path = '../Database/org_juice1.jpg';
%img2_path = '../Database/org_juice2.jpg';

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
fprintf(1, '\n');
fprintf(1, '*** Loading initial correspondences...\n');
corr = load(corr_path);
N_samples = size(corr, 1);
x1 = [corr(:, 2)'; corr(:, 1)'; ones(1, N_samples)];
x2 = [corr(:, 4)'; corr(:, 3)'; ones(1, N_samples)];
fprintf(1, 'Number of initial correspondences:\t%d\n', N_samples);
fprintf(1, 'Loaded initial correspondences\n');

%% use RANSAC to exclude outliers
fprintf(1, '\n');
fprintf(1, '*** Running RANSAC to exclude outliers...\n');
e = 0.60;       %outlier prob
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
plot(x1(2,:)', x1(1,:)','o');
epiLines2 = F'*x2;
epiLines2 = lineToBorderPoints(epiLines2, size(I1));
for idx = 1: maxInliers
    line([epiLines2(2,idx); epiLines2(4,idx)], ...
        [epiLines2(1,idx); epiLines2(3,idx)], ...
        'Color', 'r');
end
hold off;
%display 2nd image with keypoints
I2 = imread(img2_path);
figure;
imshow(I2);
hold on;
plot(x2(2,:)', x2(1,:)','o');
epiLines1 = F*x1;
epiLines1 = lineToBorderPoints(epiLines1, size(I2));
for idx = 1: maxInliers
    line([epiLines1(2,idx); epiLines1(4,idx)], ...
        [epiLines1(1,idx); epiLines1(3,idx)], ...
        'Color', 'r');
end

hold off;
%draw epipolar line corresponding to points
vgg_gui_F(I1, I2, F);
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
[P1, P2] = P_from_E(E, K, K, x1(:,1), x2(:,1));

%% Triangulation
x3D = zeros(4, maxInliers);
for i=1:maxInliers
    x3D(:,i) = LinearTriangulation(x1(:,i), x2(:,i), P1, P2);
end

%% Visualize 3D points
plot3(x3D(1,:), x3D(2,:), x3D(3,:), 'o');