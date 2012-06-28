function LeftClick_CallbackFunc(gcbo, eventdata, handles)
%% declare global variables
global K I1 I2 x1 x2 F E H12 H21 R_I1 R_I2 P1 P2 ...
    offset_x1 offset_y1 offset_x2 offset_y2 verbose ...
    click_cnt Points ratio HPoints I_new
%%
if (click_cnt == 3)
    for i=1:click_cnt
        delete(HPoints(i));
    end
    delete(HPoints(4));
    delete(HPoints(5));
    click_cnt = 0;
    return;
end

if (strcmp(get(gcf,'selectiontype'), 'alt') ~= 0)
    for i=1:click_cnt
        delete(HPoints(i));
    end
    
    click_cnt = 0;
    return;
end

click_cnt = click_cnt + 1;
p=round(get(gca,'currentpoint'));

Points(1, click_cnt) = p(1,1);
Points(2, click_cnt) = p(1,2);
Points(3, click_cnt) = 1;
HPoints(click_cnt) = plot(Points(1,click_cnt), Points(2,click_cnt), 'o', 'Color', 'r');
    
if (click_cnt < 3)
    return;
end


%% given a point in image 1, find corresponding point in image 2
%Points for school 3-4
%Points = [615 747 825;
%    480 558 452;
%    1 1 1];

% STEREO MATCHING
Corr_Points = StereoMatching(Points, F, R_I1, R_I2, H12, H21, offset_x1, offset_y1, offset_x2, offset_y2);


if (verbose==1)
%display 1st image with those points
I1 = imread(img1_path);
figure;
imshow(I1);
hold on;
for i=1:size(Points,2)
    plot(Points(1,i), Points(2,i), 'o');
end
hold off;

%display 2nd image with found points
figure;
imshow(I2);
hold on;
for i=1:size(Corr_Points,2)
    plot(Corr_Points(1,i), Corr_Points(2,i), 'o');
end
hold off;
end

%% get 3D coordinate
for i=1:size(Corr_Points,2)
    [Points_t(:,i) Corr_Points_t(:,i)] = Find_Optimal_Correspondence(Points(:,i), Corr_Points(:,i), F);
end
Points_t = round(Points_t);
Corr_Points_t = round(Corr_Points_t);
Points_3D = LinearTriangulation(Points_t, Corr_Points_t, P1, P2);
%Points_3D = LinearTriangulation(Points, Corr_Points, P1, P2);

%% METHOD 2: Client click 3 points: 2 corners and an arbitrary point on the planar

%% determine 2 other corners

%from 3 points, we determine the plane's equation
Corners_3D = zeros(4, 4);
Corners_3D(:, 1) = Points_3D(:, 1);
Corners_3D(:, 2) = Points_3D(:, 2);
Ar_Point = Points_3D(:,3);

%change coordinate to falicitate calculation
%ratio = 1; %ratio = d(point1_3D, point3_3D)/d(point2_3D, point3_3D)
T = [1 0 0 Corners_3D(1,1);
    0 1 0 Corners_3D(2,1);
    0 0 1 Corners_3D(3,1);
    0 0 0 1];
R = [vrrotvec2mat(vrrotvec([0 0 1],Corners_3D(1:3,2)-Corners_3D(1:3,1))) [0 0 0]';
    0 0 0 1];

Corners_3D_t = R\(T\Corners_3D);
Ar_Point_t = R\(T\Ar_Point);

Plane = [cross(Corners_3D_t(1:3, 1) - Ar_Point_t(1:3), Corners_3D_t(1:3, 2) - Ar_Point_t(1:3)); 
    -dot(Ar_Point_t(1:3)',cross(Corners_3D_t(1:3, 1),Corners_3D_t(1:3, 2)))];

D = norm(Corners_3D_t(:,1) - Corners_3D_t(:,2));
z_t = ratio^2/(1+ratio^2)*D;
rad = z_t/ratio;

alpha = (-Plane(4) - Plane(3)*z_t)/Plane(1);
beta = -Plane(2)/Plane(1);

y_t = roots([1+beta^2 2*alpha*beta alpha^2-rad^2]);
x_t = alpha + beta*y_t;

z_t2 = D - z_t;

Corners_3D_t(:, 3:4) = [x_t'; y_t'; z_t z_t2; 1 1];

Corners_3D = T*R*Corners_3D_t;

% rearrange corner....
t = Corners_3D(:,2);
Corners_3D(:,2) = Corners_3D(:,3);
Corners_3D(:,3) = t;

%% reproject 3D points on image 1
Corners_2D = P1*Corners_3D;
Corners_2D = HNormalize(Corners_2D);

%plot on image 1
HPoints(4) = plot(Corners_2D(1,2), Corners_2D(2,2), 'o', 'Color', 'b');
HPoints(5) = plot(Corners_2D(1,4), Corners_2D(2,4), 'o', 'Color', 'b');


% calc homography matrix 

H = H_From_4points(Corners_2D, ...
    [0 size(I_new,2) size(I_new,2) 0;... 
    size(I_new,1) size(I_new,1) 0 0;... 
    1 1 1 1]);

% warp image
minx = min(Corners_2D(1,:));
maxx = max(Corners_2D(1,:));
miny = min(Corners_2D(2,:));
maxy = max(Corners_2D(2,:));
poly = Corners_2D(1:2,:)';
I_resl = I1;
for i =minx:maxx
    for j=miny:maxy
        p = round([i j]);
        in = inpoly(p, poly);
        if (in(1) == 1)
            p_hat = H*[p 1]';
            p_hat = round(HNormalize(p_hat));
            if (p_hat(1) > 0 && p_hat(2) > 0 && p_hat(1) <= size(I_new,2) && p_hat(2)<=size(I_new,1))
                I_resl(p(2), p(1),:) = 0.5*I_new(p_hat(2), p_hat(1),:) + 0.5*I_resl(p(2), p(1),:);
            end
        end
    end
end

figure;
imshow(I_resl);

% test on image 2
Corners_2D = P2*Corners_3D;
Corners_2D = HNormalize(Corners_2D);
figure;
imshow(I2);
hold on;
plot(Corners_2D(1,:), Corners_2D(2,:), 'o', 'Color', 'b');
hold off;