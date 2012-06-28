function [H] = Proj_Trans_To_Inf(e, cx, cy)
% Compute 2D projective transformation H which map the epipole
% to a point at infinity (1, 0, 0)^T
% Input:
%   - e : original epipole
%   - (cx, cy): minimal distorted point
% Ouptut:
%   - H : 2D projective transformation

%translation matrix
T = [1 0 -cx; 0 1 -cy; 0 0 1 ];
e = T*e;

%rotation matrix
angle = atan2(e(2), e(1));
R = [cos(-angle) -sin(-angle) 0;...
    sin(-angle) cos(-angle) 0;...
    0 0 1];
e = R*e;
%G
G = [1 0 0; 0 1 0; -e(3)/e(1) 0 1];

%final result
H = G*R*T;
end