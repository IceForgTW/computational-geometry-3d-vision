function main

% create an image
     img=load('clown');
     ih=imagesc(img.X);
     axis image;
     colormap(img.map);
     cnt = 1;
% the engine (appropriate for command window usage)
    hold on;
     com=['p=get(gca,''currentpoint'');',...
          'disp(sprintf(''rachel touch me at %d/%d'',',...
          'fix(p(1,1:2))));',...
          'plot(p(1,1), p(1,2),''o'', ''Color'', ''r'');',...
          'cnt = cnt +1;',...
          'disp(cnt);'];
    set(ih,'buttondownfcn',@clicky); 
% now click on that stupid clown...

function clicky(gcbo, eventdata, handles)
disp(get(gca, 'currentpoint'));
disp(cnt);