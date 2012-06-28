function [nBestInliers, BestInliers, bestErr, iter] = PruneCorrespondences(x1, x2, e, p, s, thresh)
% Use RANSAC (with epipolar constraint) to exclude outliers
% Input:
%   - (x1, x2): correspondences
%   - e: outlier probability
%   - p: confidence
%   - s:
%   - thresh: maximum distance as inlier
% Output:
%   - nBestInliers: number of best inliers
%   - BestInliers: list of inliers
%   - bestErr

N_samples = size(x1, 2);
N = log(1-p)/log(1 - (1-e)^s); %number of maximum iterations
bestErr = Inf;
nBestInliers = 0;
iter = 0;
while (iter < N)
    iter = iter + 1;
    %randomly select training samples
    idx = randperm(N_samples);
    idx = idx(1:8); % to use 8-points algo
    %compute F
    F = F_From_8points(x1(:, idx), x2(:, idx));
    %epipolar lines
    l1 = F*x1; 
    l2 = F'*x2;
    %distance to epipolar lines
    d1 = abs(dot(x2, l1))./sqrt(l1(1,:).^2 + l1(2,:).^2);
    d2 = abs(dot(x1, l2))./sqrt(l2(1,:).^2 + l2(2,:).^2);
    %find inliers (which minimize distance to epipolar lines)
    inliers = find( d1 < thresh & d2 < thresh );
    inlierCount = size(inliers,2);            
    if inlierCount > 0
        err = sum( d1(inliers).^2 + d2(inliers).^2 ) / inlierCount;
    else
        err = Inf;
    end
    
    if ((inlierCount > nBestInliers) || (inlierCount == nBestInliers && err < bestErr))
        nBestInliers = inlierCount;
        bestErr = err;
        BestInliers = inliers;
        % Update N
        e = 1 - inlierCount / N_samples;
        if e > 0
            N = log(1 - p)/log(1 - (1 - e)^s);
        else
            N = 1;
        end
    end
end