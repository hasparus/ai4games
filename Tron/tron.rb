require 'set'

STDOUT.sync = true

def move_onto prev, nex
  if prev[0] > nex[0]
    'LEFT'
  elsif prev[0] < nex[0]
    'RIGHT'
  elsif prev[1] < nex[1]
    'DOWN'
  else 'UP'
  end
end

def log(*params)
  STDERR.puts params.map {|x| "#{x}" }.join(' | ')
end

@nboars_board = {}

def assess_value positions
  graphset = Set.new @taken
  hash = Hash[@players_count.times.map { |k| [k, {} ]}]
  players = (@my_player_id..@players_count - 1).to_a + (0..@my_player_id - 1).to_a
  turn = 1
  loop do
    full = true
    moves = {}
    for plyr in players
      for x in positions[plyr]
        for n in @nboars_board[x]
          if !graphset.include? n or (turn == 1 and moves.include? n)
            full = false
            graphset.add n
            moves[n] = plyr
          end
        end
      end
    end
    for k, v in moves
      hash[v][k] = turn
    end
    break if full
    positions = Array.new(@players_count) do |i|
      moves.select { |_, v| v == i }.keys
    end
    turn +=1
  end
  my_fields = hash[@my_player_id].length
  enemy_fields = (0..@players_count).select { |x| x != @my_player_id }.inject(&:+)
  [my_fields * 1000000, enemy_fields * -10000].inject(&:+)
end

30.times do |i|
  20.times do |j|
    neighbors = []
    if i < 29
      neighbors << [i + 1, j]
    end
    if i > 0
      neighbors << [i - 1, j]
    end
    if j < 19
      neighbors << [i, j + 1]
    end
    if j > 0
      neighbors << [i, j - 1]
    end
    @nboars_board[[i, j]] = neighbors
  end
end

@taken = {}
loop do
  @players_count, @my_player_id = gets.split(' ').collect {|x| x.to_i }
  recent_moves = []
  @players_count.times do |i|
    x0, y0, x1, y1 = gets.split(' ').collect {|x| x.to_i }
    @taken[[x0, y0]] = i
    @taken[[x1, y1]] = i
    recent_moves << [x1, y1]
  end
  recent_moves.each_with_index do |mv, i|
    @taken.keep_if { |k, v| v != i } if mv == [-1, -1]
  end
  @players_count.times do |p|
    x1, y1 = recent_moves[p]
    if p == @my_player_id
      @me = [x1, y1]
      @scores = []
      for neighbor in @nboars_board[@me]
        unless @taken.include? neighbor
          player_new_positions = recent_moves.map { |x| [x] }
          player_new_positions[@my_player_id] = [neighbor]
          recent_moves.each_with_index { |mv, i|
            player_new_positions[i] = [] if mv == [-1, -1]
          }
          score = assess_value player_new_positions
          @scores << [score, neighbor]
        end
      end
    end
  end
  best_move = @scores.sort_by { |x| -x[0] }.first
  log best_move
  log @scores
  puts move_onto(@me, best_move[-1])
end
