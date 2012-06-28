function [x1 x2] = FindCorrespondences(I1, I2, verbose)
% Find correspondences between two images with SIFT
% Input:
%   - I1: image 1
%   - I2: image 2
%   - verbose: show result
% Output:
%   - (x1, x2): correspondences

%run SIFT
I1_single = single(rgb2gray(I1));
I2_single = single(rgb2gray(I2));
[f1, d1] = vl_sift(I1_single) ;
[f2, d2] = vl_sift(I2_single) ;
%matching
[matches, ~] = vl_ubcmatch(d1, d2) ;
N_samples = size(matches, 2);
%record result
x1 = round([f1(1:2, matches(1,:)); ones(1, N_samples)]);
x2 = round([f2(1:2, matches(2,:)); ones(1, N_samples)]);

if (verbose == 1)
    figure;
    imshow(I1);
    hold on;
    plot(x1(1,:)', x1(2,:)','o');
    hold off;

    figure;
    imshow(I2);
    hold on;
    plot(x2(1,:)', x2(2,:)','o');
    hold off;
end
end