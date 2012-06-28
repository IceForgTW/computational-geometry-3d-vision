function X3D = LinearTriangulation(x1, x2, P1, P2)
% calculate 3D points of correspondence
% Input:
%   -(x1, x2): correspondence
%   - P1: camera matrix 1
%   - P2: camera matrix 2
% Output:
%   - X3D: corresponding 3D point
N_Points = size(x1, 2);

X3D = zeros(4, N_Points);
for p = 1:N_Points
    A = [x1(1,p)*P1(3,:) - P1(1,:);...
        x1(2,p)*P1(3,:) - P1(2,:);...
        x2(1,p)*P2(3,:) - P2(1,:);...
        x2(2,p)*P2(3,:) - P2(2,:)];
    [~,~,V] = svd(A);
    X3D(:,p) = V(:, end); 
end
X3D  = HNormalize(X3D);
end