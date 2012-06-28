function [Img_rsl offset_x offset_y] = homoTrans(I, H)
% apply 2d projective transformation H on I
% Input:
%   - I: original image
%   - H: transformation matrix
% Output:
%   - Img_rsl: result after transformation


[rows cols ~] = size(I);
top_left = [1 1 1]';
top_right = [cols 1 1]';
bot_left = [1 rows 1]';
bot_right = [cols rows 1]';

top_left_prime = H*top_left;
top_left_prime = top_left_prime./top_left_prime(3);
top_right_prime = H*top_right;
top_right_prime = top_right_prime./top_right_prime(3);
bot_left_prime = H*bot_left;
bot_left_prime = bot_left_prime./bot_left_prime(3);
bot_right_prime = H*bot_right;
bot_right_prime = bot_right_prime./bot_right_prime(3);

%calculate the size of result image
minx = min(round([top_left_prime(1) top_right_prime(1) bot_left_prime(1) bot_right_prime(1)]));
maxx = max(round([top_left_prime(1) top_right_prime(1) bot_left_prime(1) bot_right_prime(1)]));

miny = min(round([top_left_prime(2) top_right_prime(2) bot_left_prime(2) bot_right_prime(2)]));
maxy = max(round([top_left_prime(2) top_right_prime(2) bot_left_prime(2) bot_right_prime(2)]));

offset_x = minx-1;
offset_y = miny-1;
%initialize result image
ncols = maxx-minx+1;
nrows = maxy-miny+1;
Img_rsl = zeros(nrows, ncols, 3, 'uint8');

%transform image
for i=1:nrows
    for j=1:ncols
        ori_coor = H\([j i 1]' + [offset_x offset_y 0]');
        ori_coor = round(ori_coor./ori_coor(3));
        
        if (ori_coor(1) < 1 || ori_coor(1) > cols...
            || ori_coor(2) < 1 || ori_coor(2) > rows)
            Img_rsl(i, j,:) = 0;
        else
            Img_rsl(i, j,:) = I(ori_coor(2), ori_coor(1), :);
        end
    end
end

end