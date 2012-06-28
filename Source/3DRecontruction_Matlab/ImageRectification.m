function [H1 H2] = ImageRectification(x1, x2, F, cx, cy)
% Find 2D homography transformation matrices
% Input:
%   - (x1, x2): correspondences
%   - (cx, cy): center of image 2
% Output:
%   - H1, H2: 2D homography transformation matrices of image 1, 2

N_Points = size(x1,2);
% Get epipole in image 2
e2 = null(F');

H2 = Proj_Trans_To_Inf( e2, cx, cy);
e2 = H2*e2;
H2 = H2/e2(1); %normalize H2

[~, P2] = get_canonical_cameras(F);
M = P2(:,1:3);
H0 = H2 * M;

x2hat = H2*x2;
x2hat = HNormalize(x2hat);
x1hat = H0*x1;
x1hat = HNormalize(x1hat);

A = [ x1hat(1,:)', x1hat(2,:)', ones(N_Points,1) ];
b = x2hat(1,:)';
rsl = A\b;

HA = [rsl'; 
    0 1 0 ;
    0 0 1 ];

H1 = HA*H0;
end