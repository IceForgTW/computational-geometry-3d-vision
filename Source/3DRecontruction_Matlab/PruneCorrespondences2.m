function [nBestInliers, BestInliers, bestErr, iter] = PruneCorrespondences2(x1, x2, e, p, s, thresh)
% Use RANSAC (with homography constraint) to exclude outliers
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
    idx = idx(1:4); % to use 4-points algo
    %compute F
    H = H_From_4points(x1(:, idx), x2(:, idx));
    
    x3 = H*x1;
    x3 = HNormalize(x3);
    d = sum((x2-x3).^2,1);
    %find inliers (which minimize distance to epipolar lines)
    inliers = find( d < thresh);
    inlierCount = size(inliers,2);            
    if inlierCount > 0
        err = d;
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