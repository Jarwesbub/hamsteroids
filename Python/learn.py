import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestRegressor
import json

# -----------------------
# CONFIG
# -----------------------
WEEK_DAYS = {'Mon': 0, 'Tue': 1, 'Wed': 2, 'Thu': 3, 'Fri': 4, 'Sat': 5, 'Sun': 6}
ACTIVITIES = ['eat', 'play', 'rest', 'work', 'exercise']
DAY_NAMES = list(WEEK_DAYS.keys())
MAX_WEEKS = 6  # Keep only last 6 weeks

TRAIT_NAMES = ['discipline', 'sociability', 'energy', 'work_ethic', 'playfulness']

# -----------------------
# LOAD DATA
# -----------------------
with open('data.json', 'r') as f:
    data = json.load(f)

# Keep only last MAX_WEEKS of data
if data:
    max_week = max(entry['week'] for entry in data)
    min_week = max(0, max_week - MAX_WEEKS + 1)
    data = [entry for entry in data if entry['week'] >= min_week]

my_df = pd.DataFrame(data).fillna(0)

# Remove empty column if present
if my_df.columns[-1] == '':
    my_df = my_df.iloc[:, :-1]

print("Current data:")
print(my_df)

# -----------------------
# TRAIN MODEL
# -----------------------
my_df['day_num'] = my_df['day'].map(WEEK_DAYS)
X = my_df[['week', 'day_num']]
y = my_df[ACTIVITIES]

model = RandomForestRegressor(
    n_estimators=100,
    random_state=42,
    n_jobs=-1
)
model.fit(X, y)

# -----------------------
# CALCULATE NEXT DAY
# -----------------------
last_row = my_df.iloc[-1]
last_week = int(last_row['week'])
last_day_num = WEEK_DAYS[last_row['day']]

if last_day_num == 6:  # Sunday
    week, day_num = last_week + 1, 0
else:
    week, day_num = last_week, last_day_num + 1

day = DAY_NAMES[day_num]

print(f"\n{'='*50}")
print(f"PREDICTING NEXT DAY: Week {week}, {day}")
print(f"{'='*50}")

# -----------------------
# PREDICT ACTIVITIES
# -----------------------
prediction = model.predict([[week, day_num]])[0]
pred_values = np.maximum(
    0,
    prediction.round().astype(int) + np.random.randint(-1, 2, len(ACTIVITIES))
)

activities = {a: int(v) for a, v in zip(ACTIVITIES, pred_values)}

print(f"\nWeek {week}, {day} activities:")
for k, v in activities.items():
    print(f"  {k}: {v}")

# -----------------------
# TRAIT ACCUMULATION
# -----------------------

# Update traits
def update_traits(previous_traits, today_activities):
    if previous_traits is None:
        previous_traits = {t: 5 for t in TRAIT_NAMES}  # mid-value start

    traits = {k: int(v) for k, v in previous_traits.items()}

    # Updated rules
    traits['discipline'] += today_activities['work'] - today_activities['play']
    traits['sociability'] += today_activities['play'] - today_activities['work']
    traits['energy'] += today_activities['rest'] - today_activities['exercise']
    traits['work_ethic'] += today_activities['work'] + today_activities['exercise'] - today_activities['play']
    traits['playfulness'] += today_activities['play'] - today_activities['work']

    # Clamp 0-10
    for k in traits:
        traits[k] = max(0, min(10, traits[k]))

    return traits

# Get previous traits from last entry if present
if len(my_df) > 0 and 'traits' in my_df.columns:
    previous_traits = my_df.iloc[-1]['traits']
else:
    previous_traits = None

traits = update_traits(previous_traits, activities)

# -----------------------
# PERSONA / BACKGROUND (flavor)
# -----------------------
def personality_summary(traits, activities):
    # energy low -> lazy
    energy_desc = "low" if traits['energy'] < 4 else "high"
    work_ethic_desc = "high" if traits['work_ethic'] >= 5 else "low"
    sociability_desc = "high" if traits['sociability'] >= 5 else "low"
    discipline_desc = "high" if traits['discipline'] >= 5 else "low"

    background = "Learned some bad habits due to neglect" if traits['discipline'] < 4 else "Well-raised with good routines"

    return {
        "energy": energy_desc,
        "work_ethic": work_ethic_desc,
        "sociability": sociability_desc,
        "discipline": discipline_desc,
        "background": background
    }

personality = personality_summary(traits, activities)

# -----------------------
# SAVE NEXT ENTRY
# -----------------------
new_entry = {
    "week": int(week),
    "day": day,
    **{a: int(v) for a, v in activities.items()},
    "traits": {k: int(v) for k, v in traits.items()},
    "personality": personality
}

with open('data.json', 'r+') as f:
    data = json.load(f)
    data.append(new_entry)
    f.seek(0)
    json.dump(data, f, indent=2)
    f.truncate()

print("\nTraits summary:")
for k, v in traits.items():
    print(f"  {k}: {v}")

print("\nPersonality summary:")
for k, v in personality.items():
    print(f"  {k}: {v}")

print("\nPrediction added to data.json")
