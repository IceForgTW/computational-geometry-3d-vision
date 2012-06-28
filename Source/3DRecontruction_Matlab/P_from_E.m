function [P1, P2] = P_from_E(E, K1, K2, x1, x2)
% Extract camera matrices from essential matrix
% Input:
%   -E: essential matrix
%   -K1: intrinsic parameters of camera 1
%   -K2: intrinsic parameters of camera 2
%   -(x1, x2): correspondence used to determine reasonable solution
% Output:
%   -P1: camera matrix 1
%   -P2: camera matrix 2

%fix camera matrix 1
P1 = K1*[eye(3) zeros(3, 1)];

%determine 4 possibilities for P2
[U, S, V] = svd(E);
u3 =  U(:, 3);
W = [0 -1 0; 1 0 0; 0 0 1];

P2t = zeros(3, 4, 4);
P2t(:,:,1) = [U*W*V' u3];
P2t(:,:,2) = [U*W*V' -u3];
P2t(:,:,3) = [U*W'*V' u3];
P2t(:,:,4) = [U*W'*V' -u3];

%just one of 4 possibilities is feasible
X3D = zeros(4, 4);
P2 = zeros(3, 4);
for i=1:4
    X3D(:, i) = LinearTriangulation(x1, x2, P1, K2*P2t(:,:,i));
    
    T = X3D(end, i);
    %P = [M|p4]
    M = P1(:,1:3);
    %P(X,Y,Z,T)' = w(x,y,1)'
    xi = P1*X3D(:, i);
    w = xi(3);
    m3 = M(3,:);
    s1 = sign((sign(det(M))*w)/(T*norm(m3)));
    
    %P = [M|p4]
    M = P2t(:,1:3,i);
    %P(X,Y,Z,T)' = w(x,y,1)'
    xi = P2t(:,:,i)*X3D(:, i);
    w = xi(3);
    m3 = M(3,:);
    s2 = sign((sign(det(M))*w)/(T*norm(m3)));
    
    if (s1 > 0 && s2 > 0)
        P2 = P2t(:,:,i);
        break;
    end
end