function F = F_From_8points(x1, x2)
%Calculate the fundamental matrix from 8 points
%Input: x1 <-> x2 are the correspondences
%Output: F: fundamental matrix

%determine linear solution
A = [ repmat(x2(1,:)',1,3) .* x1', repmat(x2(2,:)',1,3) .* x1', x1(1:3,:)'];
[U,S,V] = svd(A);
F = reshape(V(:,end),3,3)';

%contraint enforcement (because F must be singular)
[U,S,V] = svd(F);
F = U*diag([S(1) S(5) 0])*(V');
end