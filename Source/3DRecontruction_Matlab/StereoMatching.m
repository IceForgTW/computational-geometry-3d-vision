function Corr_Points = StereoMatching(Points, F, RImg1, RImg2, H1, H2, offset_x1, offset_y1, offset_x2, offset_y2)
% Find corresponding points in image 2 given points in image 1
% Input:
%   - Points: list of points in image 1
%   - F: fundamental matrix
%   - RImg1: rectified image 1
%   - RImg2: rectified image 2

N_Points = size(Points, 2); %number of points

%get corresponding points in rectified image 1
Points_hat = H1*Points;
Points_hat = Points_hat./repmat(Points_hat(3,:), 3, 1);
Points_hat = Points_hat - repmat([offset_x1 offset_y1 0]', 1, N_Points);

%get corresponding epipolar line in rectified image 2
T = [1 0 -offset_x2; 0 1 -offset_y2; 0 0 1 ];
corr_epiLines_hat = inv(T)'*inv(H2)'*F*Points;
corr_epiLines_hat = lineToBorderPoints(corr_epiLines_hat, [size(RImg2,2) size(RImg2,1) 3]);

%line search
window_rad = 30; %rad of searching window
row_search = round(corr_epiLines_hat(2,:));
Points_hat = round(Points_hat);

col_search = zeros(1, N_Points);

for p=1:N_Points
    max_ncc = -Inf;
    max_idx = -1;
    for c = 1:size(RImg2,2)
        if (c - window_rad <= 0 || row_search(p) - window_rad <= 0 ...
                || c + window_rad > size(RImg2,2) || row_search(p) + window_rad > size(RImg2,1) ...
                || Points_hat(1,p) - window_rad <= 0 || Points_hat(2,p) - window_rad <= 0 ...
                || Points_hat(1,p) + window_rad > size(RImg1,2) || Points_hat(2,p) + window_rad > size(RImg1,1))
            continue;
        else
            ab = 0;
            a = 0;
            b = 0;
            for i=-window_rad:window_rad
                for j = -window_rad:window_rad
                    ab = ab + double(RImg1(Points_hat(2,p) + i, Points_hat(1,p) + j))*double(RImg2(row_search(p)+i,c+j));
                    a = a + double(RImg1(Points_hat(2,p) + i, Points_hat(1,p) + j))^2;
                    b = b + double(RImg2(row_search(p)+i,c+j))^2;
                end
            end
            ncc = ab/sqrt(a*b);
            if (ncc > max_ncc)
                max_ncc = ncc;
                max_idx = c;
            end
        end
    end
    col_search(p) = max_idx;
end
Corr_Points = H2\([col_search; row_search; ones(1, N_Points)] + repmat([offset_x2 offset_y2 0]', 1, N_Points));
Corr_Points = round(HNormalize(Corr_Points));
end