function F = F_From_normalized_8points(x1, x2)
%Calculate the fundamental matrix from 8 points
%Input: x1 <-> x2 are the correspondences
%Output: F: fundamental matrix

%normalization
T1 = get_normalization_matrix(x1);
T2 = get_normalization_matrix(x2);
x1 = T1*x1;
x2 = T2*x2;

F = F_From_8points(x1, x2);

%denormalization
F= T2'*F*T1;
end