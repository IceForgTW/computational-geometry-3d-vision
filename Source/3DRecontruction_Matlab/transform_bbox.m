function box = transform_bbox( H, rows, cols )

  box = [ 1 cols 1 cols ; 1 1 rows rows ; 1 1 1 1 ];
  box = H * box;
  box = round( box(1:2,:) ./ repmat( box(3,:), 2, 1 ));
end