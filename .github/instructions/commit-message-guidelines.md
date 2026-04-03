---
description: 'Commit message guidelines for Copilot'
applyTo: 'Use this instruction whenever the user asks for a git commit message'
---

- Base the commit message on the git diff compared to the last commit (describe the main changes relative to HEAD).
- The commit message has to be in english.
- After being done with this instruction continue communication with the user in german.
- First line: short imperative summary (<= 50 characters).
- Optional body: 1–6 short bullet-like lines (each starting with "- ") listing the main changes.
- Do NOT include filenames, long explanations, or footers.
- Do NOT append Co-authored-by or any other trailers.
- If context is missing, ask one short clarifying question before generating the message.
- When asked, output only the commit message text (no code block, no extra commentary).