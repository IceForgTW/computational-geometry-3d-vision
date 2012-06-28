function errP = TuneP(x1,P1,x2,P2)
   XwEst = Triangulation(x1,P1,x2,P2);
   xhat = P1*XwEst; xhat = xhat./repmat(xhat(3,:),3,1);
   xphat = P2*XwEst; xphat = xphat./repmat(xphat(3,:),3,1);
   ErrImg1 = sum(sum((xhat-x1).*(xhat-x1)));
   ErrImg2 = sum(sum((xphat-x2).*(xphat-x2)));
   errP = sqrt((ErrImg1+ErrImg2)/size(x1,2));